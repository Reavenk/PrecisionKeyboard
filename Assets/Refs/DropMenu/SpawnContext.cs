using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace DropMenu
    {
        public class SpawnContext
        { 
            public enum ActionBuffer
            {
                SwitchToLeft,
                SwitchToRight,
                SwitchDir,
                TopLeftMenuToBotLeftInvoker,
                TopRightMenuToBotRightInvoker,
                TopLeftMenuToTopRightInvoker,
                TopRightMenuToTopLeftInvoker,
                TryAlignRightMenuToLeftInvoker,
                TryAlignLeftMenuToRightInvoker,
                FlushLeft,
                FlushRight,
                FitInBounds
            }
            /// <summary>
            /// When spawning menus, do submenus spawn to the left or the right. 
            /// We use this to keep track of that state. If we get too far into
            /// one direction, we will switch the direction which things spawn.
            /// </summary>
            public enum SpawnDirection
	        { 
		        Left,   // Submenus spawn to the left of the parent menu.
		        Right   // Submenus spawn to the right of the parent menu.
	        }

            /// <summary>
            /// A structure to group menu nodes and asset data together while
            /// creating the menus.
            /// </summary>
            protected struct NodeCreationCache
            {
                public Node node;
                public UnityEngine.UI.Image plate;
                public UnityEngine.UI.Text text;
                public UnityEngine.UI.Image icon;   // Will be null if doesn't have an icon.
                public UnityEngine.UI.Image arrow;  // Will be null if not a submenu
                public float height;

                public NodeCreationCache(Node node)
                { 
                    this.node = node;

                    this.plate = null;
                    this.text = null;
                    this.icon = null;
                    this.arrow = null;
                    this.height = 0.0f;
                }
            }

            public class NodeContext
            { 
                public Node menu;                   // The menu representation
                public UnityEngine.UI.Image plate;  // The UI container for the entire image
                public UnityEngine.UI.Image shadow; // The shadow behind the plate
                public bool hasScroll = false;

                public NodeContext(Node menu, UnityEngine.UI.Image plate, UnityEngine.UI.Image shadow)
                { 
                    this.menu = menu;
                    this.plate = plate;
                    this.shadow = shadow;
                }

                public void LayoutShadow(Props props, Transform tBotPlate)
                {
                    if(props == null || this.shadow == null)
                        return;

                    RectTransform rtMain = this.plate.rectTransform;

                    RectTransform rtShadow = this.shadow.rectTransform;
                    //
                    rtShadow.sizeDelta          = rtMain.sizeDelta;
                    rtShadow.anchoredPosition   = Vector2.zero;
                    rtShadow.position           = rtMain.position + (Vector3)props.shadowOffset;
                    // move shadow to top
                    if(tBotPlate != null)
                        rtShadow.SetSiblingIndex( tBotPlate.GetSiblingIndex() - 1);
                    else
                        rtShadow.SetSiblingIndex( rtMain.GetSiblingIndex() - 1);
                }
            }

            public DropMenuSpawner spawner;
            private RectTransform modalPlate;

            public SpawnDirection spawnDirection = SpawnDirection.Right;

            public List<NodeContext> spawnedSubmenus = 
                new List<NodeContext>();

            public SpawnContext(
                Canvas canvas,
                DropMenuSpawner spawner,
                SpawnDirection spawnDirection = SpawnDirection.Right)
            { 
                this.spawner = spawner;
                this.spawnDirection = spawnDirection;

                //      CREATE MODAL PLATE
                //////////////////////////////////////////////////

                GameObject goMP = new GameObject("ModalPlate");
                goMP.transform.SetParent(canvas.transform);
                goMP.transform.localScale = Vector3.one;
                goMP.transform.localRotation = Quaternion.identity;
                UnityEngine.UI.Image imgBP = goMP.AddComponent<UnityEngine.UI.Image>();
                imgBP.color = spawner.props.modalPlateColor;
                RectTransform rtBP = imgBP.rectTransform;
                rtBP.anchorMin = Vector2.zero;
                rtBP.anchorMax = Vector2.one;
                rtBP.offsetMin = Vector2.zero;
                rtBP.offsetMax = Vector2.zero;
                rtBP.pivot = new Vector2(0.0f, 1.0f);
                UnityEngine.EventSystems.EventTrigger tMP = goMP.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                tMP.triggers = new List<UnityEngine.EventSystems.EventTrigger.Entry>();
                UnityEngine.EventSystems.EventTrigger.Entry mpClickExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                mpClickExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
                mpClickExit.callback = new UnityEngine.EventSystems.EventTrigger.TriggerEvent();
                mpClickExit.callback.AddListener(
                    (x) => 
                    { 
                        this.spawner.onAction?.Invoke(null);
                        this.Destroy();
                    });
                tMP.triggers.Add(mpClickExit);

                this.spawner.onCreatedModalPlate?.Invoke(imgBP);
                    
                //      SET ROOT PLATE
                //////////////////////////////////////////////////

                this.modalPlate = rtBP;
            }

            public void BreakDown(int depth)
            {
                while(this.spawnedSubmenus.Count >= depth + 1)
                    this.PopMenu();
            }

            public void Destroy()
            { 
                if(this.modalPlate.gameObject != null)
                    GameObject.Destroy(this.modalPlate.gameObject);
            }

            public bool PopMenu(NodeContext context, bool checkFirst = true)
            { 
                return this.PopMenu(context.plate.rectTransform , checkFirst);
            }

            public bool PopMenu(Node node, bool checkFirst = true)
            { 
                if(checkFirst == true)
		        { 
			        bool found = false;
			        foreach(NodeContext nctx in this.spawnedSubmenus)
			        { 
				        if(nctx.menu == node)
				        {
					        found = true;
					        break;
				        }
			        }
			        if(found == false)
				        return false;
		        }

		        while(this.spawnedSubmenus.Count > 0)
		        { 
			        NodeContext nctxLast = 
                        this.spawnedSubmenus[this.spawnedSubmenus.Count - 1];

			        if(nctxLast.menu == node)
                        return true;

			        this.PopMenu();
		        }

		        return false;
            }

            public bool PopMenu(RectTransform toRoot, bool checkFirst = true)
	        { 
		        if(checkFirst == true)
		        { 
			        bool found = false;
			        foreach(NodeContext nctx in this.spawnedSubmenus)
			        { 
				        if(nctx.plate.rectTransform == toRoot)
				        {
					        found = true;
					        break;
				        }
			        }
			        if(found == false)
				        return false;
		        }

		        while(this.spawnedSubmenus.Count > 0)
		        { 
			        NodeContext nctxLast = 
                        this.spawnedSubmenus[this.spawnedSubmenus.Count - 1];

			        if(nctxLast.plate.rectTransform == toRoot)
                        return true;

			        this.PopMenu();
		        }

		        return false;
	        }

	        public bool PopMenu()
	        { 
		        if(this.spawnedSubmenus.Count != 0)
		        { 
			        int lastId = this.spawnedSubmenus.Count - 1;
			        NodeContext nctx = this.spawnedSubmenus[lastId];

			        this.spawnedSubmenus.RemoveAt(lastId);
			        GameObject.Destroy(nctx.plate.gameObject);

			        if(nctx.shadow != null)
				        GameObject.Destroy(nctx.shadow.gameObject);
		        }

		        if(this.spawnedSubmenus.Count == 0)
                    this.Destroy();

		        return true;
	        }

            public NodeContext CreateDropdownMenu(Node menu, RectTransform rtInvokingRect)
            {
                Vector3 [] corners = new Vector3[4];
                rtInvokingRect.GetWorldCorners(corners);

                ActionBuffer [] rabs = null;
                if(this.spawnDirection == SpawnDirection.Left)
                { 
                    rabs = 
                            new ActionBuffer[]
                            { 
                                ActionBuffer.TopRightMenuToBotRightInvoker,
                                ActionBuffer.SwitchToRight,
                                ActionBuffer.TopLeftMenuToBotLeftInvoker,
                                ActionBuffer.FlushLeft,
                                ActionBuffer.FitInBounds
                            };
                }
                else
                { 
                    rabs = 
                        new ActionBuffer[]
                        { 
                            ActionBuffer.TopLeftMenuToBotLeftInvoker,
                            ActionBuffer.SwitchToLeft,
                            ActionBuffer.TopRightMenuToBotRightInvoker,
                            ActionBuffer.FlushRight,
                            ActionBuffer.SwitchToLeft,
                            ActionBuffer.FlushLeft,
                            ActionBuffer.FitInBounds
                        };
                }

                NodeContext ret = 
                    this.CreateDropdownMenu(menu, corners, false, rabs);

                return ret;
            }

            public NodeContext CreateDropdownSubMenu(Node menu, bool pushScroll, RectTransform rtInvokingRect)
            {
                Vector3 [] corners = new Vector3[4];
                rtInvokingRect.GetWorldCorners(corners);

                if(pushScroll == true)
                {
                    float scrW = 
                        this.spawner.props.scrollbarWidth + 
                        this.spawner.props.outerPadding.right;

                    float scrollbarOffset = scrW * rtInvokingRect.lossyScale.x;
                    corners[2].x += scrollbarOffset;
                    corners[3].x += scrollbarOffset;
                }

                ActionBuffer [] rabs = null;
                if(this.spawnDirection == SpawnDirection.Left)
                { 
                    rabs = 
                            new ActionBuffer[]
                            { 
                                ActionBuffer.TopRightMenuToTopLeftInvoker,
                                ActionBuffer.TopRightMenuToTopLeftInvoker,
                                ActionBuffer.SwitchToRight,
                                ActionBuffer.FlushLeft,
                                ActionBuffer.FitInBounds
                            };
                }
                else
                { 
                    rabs = 
                        new ActionBuffer[]
                        { 
                            ActionBuffer.TopLeftMenuToTopRightInvoker,
                            ActionBuffer.TopRightMenuToTopLeftInvoker,
                            ActionBuffer.SwitchToLeft,
                            ActionBuffer.FlushRight,
                            ActionBuffer.FlushLeft,
                            ActionBuffer.FitInBounds
                        };
                }

                NodeContext ret = 
                    this.CreateDropdownMenu(
                        menu, 
                        corners, 
                        this.spawner.props.useGoBack == true, 
                        rabs);

                this.spawner?.onSubMenuOpened(this, ret);

                return ret;
            }

            private NodeContext CreateDropdownMenu(Node node, Vector3 [] bounds, bool addGoBack, params ActionBuffer [] buffers)
            { 
                if(buffers == null || buffers.Length == 0)
                    buffers = new ActionBuffer[]{ ActionBuffer.TopLeftMenuToBotLeftInvoker };

                // Note that we only care about adjusting the horizontal positioning of 
                // the created menu, the deffered CreateDropdownMenu() handles post 
                // processing the Y position (and adding a vert scroll is needed).

                // Layout plate
                // Figure out if the menu needs to be adjusted
                Vector3[] canvasCorners = new Vector3[4];
                this.modalPlate.GetWorldCorners(canvasCorners);
                Vector2 canvasDim = canvasCorners[2] - canvasCorners[0];

                // What we're mainly conerned about right now is that the Y value is in the correct
                // location if it doesn't end up needing to adjust.
                Vector3 initialTry = bounds[0];
                switch(buffers[0])
                { 
                    case ActionBuffer.TopLeftMenuToTopRightInvoker:
                    case ActionBuffer.TopRightMenuToTopLeftInvoker:
                        initialTry = bounds[1];
                        break;
                }

                NodeContext nctx = CreateDropdownMenu(node, addGoBack, initialTry);

                Vector3[] menuCorners = new Vector3[4];
                nctx.plate.rectTransform.GetWorldCorners(menuCorners);
                Vector3 menuDim = menuCorners[2] - menuCorners[0];
                Vector3 menuTL = menuCorners[1];
            
                foreach(ActionBuffer abCmd in buffers)
                { 
                    
                    switch(abCmd)
                    { 
                        case ActionBuffer.SwitchToLeft:
                            this.spawnDirection = SpawnDirection.Left;
                            continue;

                        case ActionBuffer.SwitchToRight:
                            this.spawnDirection = SpawnDirection.Right;
                            continue;

                        case ActionBuffer.SwitchDir:
                            this.spawnDirection = 
                                (this.spawnDirection == SpawnDirection.Left) ? 
                                    SpawnDirection.Right : 
                                    SpawnDirection.Left;
                            continue;

                        case ActionBuffer.TopLeftMenuToBotLeftInvoker:
                            { 
                                float tm = bounds[0].x - menuCorners[0].x;
                                nctx.plate.transform.position += new Vector3(tm, 0.0f, 0.0f);
                            }
                            break;

                        case ActionBuffer.TopLeftMenuToTopRightInvoker:
                            { 
                                float tm = bounds[2].x - menuCorners[0].x;
                                nctx.plate.transform.position += new Vector3(tm, 0.0f, 0.0f);
                            }
                            break;

                        case ActionBuffer.TopRightMenuToBotRightInvoker:
                            { 
                                float tm = bounds[2].x - menuCorners[2].x;
                                nctx.plate.transform.position += new Vector3(tm, 0.0f, 0.0f);
                            }
                            break;

                        case ActionBuffer.TopRightMenuToTopLeftInvoker:
                            { 
                                float tm = bounds[0].x - menuCorners[2].x;
                                nctx.plate.transform.position += new Vector3(tm, 0.0f, 0.0f);
                            }
                            break;

                        case ActionBuffer.TryAlignRightMenuToLeftInvoker:
                            {
                                float tm = menuCorners[2].x - bounds[0].x;
                                nctx.plate.transform.position += new Vector3(tm, 0.0f, 0.0f);
                            }
                            break;

                        case ActionBuffer.TryAlignLeftMenuToRightInvoker:
                            {
                                float tm = bounds[2].x - menuCorners[0].x;
                                nctx.plate.transform.position += new Vector3(tm, 0.0f, 0.0f);
                            }
                            break;

                        case ActionBuffer.FlushLeft:
                            nctx.plate.transform.position -= 
                                new Vector3(
                                    menuCorners[0].x, 
                                    0.0f, 
                                    0.0f);
                            break;

                        case ActionBuffer.FlushRight:
                            nctx.plate.transform.position += 
                                new Vector3(
                                    menuCorners[2].x - menuCorners[0].x, 
                                    0.0f, 
                                    0.0f);
                            break;

                        // This is the last resort, nothing should be after this because 
                        // it invalidates our cached width
                        case ActionBuffer.FitInBounds:
                            // TODO: Figure out later
                            //nctx.plate.transform.position -= new Vector3(menuCorners[0].x, 0.0f, 0.0f);
                            //nctx.plate.rectTransform.sizeDelta = 
                            break;
                    }

                    nctx.plate.rectTransform.GetWorldCorners(menuCorners);
                    if(menuCorners[0].x >= canvasCorners[0].x && menuCorners[2].x <= canvasCorners[2].x)
                        break;
                }

                //If the menu is going off the right side, have it spawn with the right
                //corner aligned to the right of the rectangle
                if (menuTL.x + menuDim.x > canvasDim.x)
                    menuTL.x = canvasCorners[3].x - menuDim.x;
            
                // If the menu is going off the left side, dock it to the left
                if(menuTL.x < canvasCorners[0].x)
                    menuTL.x = canvasCorners[0].x;
            
                // If the menu is going off the right side, dock it to the right.
                //if (menuTL.x + menuDim.x > canvasDim.x)
                //    menuTL.x += canvasDim.x - menuTL.x - menuDim.x;
                //
                //nctx.plate.rectTransform.position += menuTL - menuCorners[1];

                Transform baseForShadowZ = null;
                if(this.spawnedSubmenus.Count >= 2)
                    baseForShadowZ = this.spawnedSubmenus[this.spawnedSubmenus.Count - 2].plate.transform;
                //
                nctx.LayoutShadow(this.spawner.props,baseForShadowZ);
            
                //this.onCreate?.Invoke(nctx);
            
                return nctx;
            }

            public NodeContext CreateDropdownMenu(Node node, bool addGoBack, Vector3 topLeftSpawn)
            { 
                NodeContext nctx = this._CreateDropdownMenu(node, addGoBack, topLeftSpawn);
                
                //this.onCreate?.Invoke(nctx);
                return nctx;
            }

            IEnumerable<Node> GetMenuNodes(Node menu, bool addGoBack, NodeContext nctxGoBackTo)
            { 
                if(addGoBack == true)
                { 
                    Node nGoBack = 
                        new Node(
                            "Go Back", 
                            ()=>
                            { 
                                this.PopMenu(menu); // Go to what menu spawned us
                                this.PopMenu();     // And then one back
                            });

                    nGoBack.type = Node.Type.GoBack;
                    nGoBack.sprite = this.spawner.props.goBackIcon;

                    yield return nGoBack;
                }

                foreach(Node n in menu.children)
                    yield return n;
            }

            protected NodeContext _CreateDropdownMenu(Node node, bool addGoBack, Vector3 topLeftSpawn)
            {
                Props props = this.spawner.props;

                GameObject goMenu = new GameObject("Menu");
                goMenu.transform.SetParent(this.modalPlate, false);
                UnityEngine.UI.Image imgMenu = goMenu.AddComponent<UnityEngine.UI.Image>();
                imgMenu.sprite = props.plate;
                imgMenu.type = UnityEngine.UI.Image.Type.Sliced;
                RectTransform rtMenu = imgMenu.rectTransform;
                rtMenu.pivot = new Vector2(0.0f, 1.0f);
                rtMenu.anchorMin = new Vector2(0.0f, 1.0f);
                rtMenu.anchorMax = new Vector2(0.0f, 1.0f);
                
                GameObject goShadow = new GameObject("Shadow");
                goShadow.transform.SetParent(this.modalPlate, false);
                UnityEngine.UI.Image imgShadow = goShadow.AddComponent<UnityEngine.UI.Image>();
                imgShadow.sprite = props.shadow;
                imgShadow.type = UnityEngine.UI.Image.Type.Sliced;
                imgShadow.color = props.shadowColor;
                RectTransform rtShadow = imgShadow.rectTransform;
                rtShadow.pivot = new Vector2(0.0f, 1.0f);
                rtShadow.anchorMin = new Vector2(0.0f, 1.0f);
                rtShadow.anchorMax = new Vector2(0.0f, 1.0f);

                float maxXIco = 0.0f;
                float maxXLabel = 0.0f;
                float maxXSpawnArrow = 0.0f;

                bool atleastOne = false;

                NodeContext nctxRet = 
                    new NodeContext(
                        node,
                        imgMenu,
                        imgShadow);

                this.spawnedSubmenus.Add(nctxRet);

                // Create assets and get total size values
                List< NodeCreationCache> childrenCreationCache = new List<NodeCreationCache>();
                foreach (Node n in this.GetMenuNodes(node, addGoBack, nctxRet))
                {
                    float fyMax = 0.0f;

                    NodeCreationCache ncc = new NodeCreationCache(n);
                    if (n.type == Node.Type.Action || n.type == Node.Type.Menu || n.type == Node.Type.GoBack)
                    {
                        Node nCpy = n;

                        GameObject goEntry = new GameObject("Entry");
                        goEntry.transform.SetParent(goMenu.transform, false);
                        UnityEngine.UI.Image imgEntry = goEntry.AddComponent<UnityEngine.UI.Image>();
                        imgEntry.sprite = props.entrySprite;
                        imgEntry.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
                        imgEntry.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
                        imgEntry.rectTransform.pivot = new Vector2(0.0f, 1.0f);
                        imgEntry.type = UnityEngine.UI.Image.Type.Sliced;

                        ncc.plate = imgEntry;

                        GameObject goText = new GameObject("Text");
                        goText.transform.SetParent(goEntry.transform, false);
                        UnityEngine.UI.Text txtText = goText.AddComponent<UnityEngine.UI.Text>();
                        txtText.color = props.entryFontColor;
                        txtText.fontSize = props.entryFontSize;
                        txtText.font = props.entryFont;
                        txtText.text = n.label;
                        txtText.horizontalOverflow = HorizontalWrapMode.Overflow;
                        txtText.verticalOverflow = VerticalWrapMode.Overflow;
                        txtText.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                        txtText.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
                        txtText.rectTransform.pivot = new Vector2(0.0f, 0.5f);
                        txtText.alignment = props.GetTextAnchorFromAlignment(nCpy.alignment, true);
                        TextGenerationSettings tgs = txtText.GetGenerationSettings(new Vector2(float.PositiveInfinity, float.PositiveInfinity));
                        tgs.scaleFactor = 1.0f;
                        TextGenerator tg = txtText.cachedTextGenerator;
                        Vector2 textSz = 
                            new Vector2(
                                Mathf.Ceil(tg.GetPreferredWidth(txtText.text, tgs) + 1.0f / goText.transform.lossyScale.x), 
                                tg.GetPreferredHeight(txtText.text, tgs));
                        txtText.rectTransform.sizeDelta = textSz;
                        //
                        maxXLabel = Mathf.Max(maxXLabel, textSz.x);
                        fyMax = Mathf.Max(fyMax, textSz.y, props.minEntrySize);
                        //
                        ncc.text = txtText;

                        UnityEngine.UI.Button itemBtn = null;
                        if(n.type == Node.Type.Action || n.type == Node.Type.GoBack)
                        { 
                            UnityEngine.UI.Button actionBtn = goEntry.AddComponent<UnityEngine.UI.Button>();
                            itemBtn = actionBtn;

                            actionBtn.targetGraphic = ncc.plate;
                            actionBtn.onClick.AddListener(
                                ()=>
                                { 
                                    n.action?.Invoke();

                                    if(n.type != Node.Type.GoBack)
                                    {
                                        this.spawner.onAction?.Invoke(nCpy);
                                        this.Destroy();
                                    }
                                });

                            goEntry.ETQ().AddOnPointerEnter(
                                (x)=>
                                { 
                                    this.PopMenu(node); 
                                });

                        }
                        else if(n.type == Node.Type.Menu)
                        { 
                            // Do nothing with the button. We're just leveraging the 
                            // hover-over animations.
                            UnityEngine.UI.Button menuBtn = goEntry.AddComponent<UnityEngine.UI.Button>();
                            itemBtn = menuBtn;

                            menuBtn.targetGraphic = ncc.plate;

                            goEntry.ETQ().AddOnPointerEnter(
                                (x)=>
                                {
                                    //// If the menu is already , pop children menus all the way back to it
                                    //if(this.PopMenu(nCpy) == false)
                                    //{
                                    //    // If not, another submenu might be up that we need to get rid of to make space
                                    //    // for this new submenu - in which case we pop back to the menu that is about to
                                    //    // spawn this submenu.
                                    //    this.PopMenu(node);
                                    //}

                                    if(this.PopMenu(nCpy) == true)
                                        return;


                                    this.CreateDropdownSubMenu(
                                        nCpy,
                                        nctxRet.hasScroll,
                                        imgEntry.rectTransform);
                                });
                        }

                        ncc.text = txtText;

                        if (n.sprite != null)
                        { 
                            Vector2 v2Ico = n.sprite.rect.size;

                            GameObject goIcon = new GameObject("Icon");
                            goIcon.transform.SetParent(goEntry.transform);
                            goIcon.transform.localRotation = Quaternion.identity;
                            goIcon.transform.localScale = Vector3.one;
                            UnityEngine.UI.Image imgIcon = goIcon.AddComponent<UnityEngine.UI.Image>();
                            imgIcon.sprite = n.sprite;
                            imgIcon.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
                            imgIcon.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
                            imgIcon.rectTransform.pivot = new Vector2(0.0f, 1.0f);
                            imgIcon.rectTransform.sizeDelta = v2Ico;

                            ncc.icon = imgIcon;

                            maxXIco = Mathf.Max(v2Ico.x, maxXIco);
                            fyMax   = Mathf.Max(v2Ico.y, fyMax);
                        }

                        Color cUse = props.unselectedColor;
                        //
                        if((n.flags & Node.Flags.Colored) != 0)
                            cUse = n.color;
                        else if((n.flags & Node.Flags.Selected) != 0)
                            cUse = props.selectedColor;
                        //
                        imgEntry.color = cUse;

                        if(itemBtn != null)
                        {
                            if((n.flags & Node.Flags.Disabled) != 0)
                                itemBtn.interactable = false;

                            UnityEngine.UI.ColorBlock cb = itemBtn.colors;
                            cb.normalColor = cUse;
                            cb.highlightedColor *= cUse;
                            cb.pressedColor *= cUse;
                            cb.disabledColor *= cUse;
                            itemBtn.colors = cb;
                        }

                        if(n.type == Node.Type.Menu)
                        {
                            Vector2 v2Arrow = props.submenuSpawnArrow.rect.size;

                            GameObject goArrow = new GameObject("Arrow");
                            goArrow.transform.SetParent(goEntry.transform);
                            goArrow.transform.localRotation = Quaternion.identity;
                            goArrow.transform.localScale = Vector3.one;
                            UnityEngine.UI.Image imgArrow = goArrow.AddComponent<UnityEngine.UI.Image>();
                            imgArrow.sprite = props.submenuSpawnArrow;
                            imgArrow.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
                            imgArrow.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
                            imgArrow.rectTransform.pivot = new Vector2(0.0f, 1.0f);
                            imgArrow.rectTransform.sizeDelta = v2Arrow;

                            ncc.arrow = imgArrow;

                            maxXSpawnArrow = Mathf.Max(maxXSpawnArrow, v2Arrow.x);
                            fyMax = Mathf.Max(fyMax, v2Arrow.y);
                        }

                        fyMax += props.entryPadding.top + props.entryPadding.bottom;
                        ncc.height = fyMax;
                    }
                    else if(n.type == Node.Type.Separator)
                    { 
                        float stotalX = props.minSplitter + props.splitterPadding.left + props.splitterPadding.right;
                        float stotalY = props.splitter.rect.height + props.splitterPadding.top + props.splitterPadding.bottom;

                        GameObject goSep = new GameObject("Separator");
                        goSep.transform.SetParent(goMenu.transform);
                        goSep.transform.localRotation = Quaternion.identity;
                        goSep.transform.localScale = Vector3.one;
                        UnityEngine.UI.Image imgSpe = goSep.AddComponent<UnityEngine.UI.Image>();
                        imgSpe.sprite = props.splitter;
                        imgSpe.type = UnityEngine.UI.Image.Type.Sliced;
                        imgSpe.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
                        imgSpe.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
                        imgSpe.rectTransform.pivot = new Vector2(0.0f, 1.0f);

                        ncc.plate = imgSpe;

                        ncc.height = 
                            props.splitterPadding.top + 
                            props.splitterPadding.bottom + 
                            props.splitter.rect.height;
                    }
                    else
                    { 
                        // TODO: Error
                        continue;
                    }

                    childrenCreationCache.Add(ncc);
                }

                maxXLabel = Mathf.Ceil(maxXLabel) + 1.0f;

                // Layout internals
                float fY = props.outerPadding.top;

                float fX = props.entryPadding.left;
                float xIcoStart = fX;
                float xTextStart = fX + maxXIco;
                // Offset where the text starts
                if(maxXIco != 0.0f)
                    xTextStart += props.iconTextPadding;

                // Define where the arrow would start
                float xArrowIco = xTextStart + maxXLabel;
                float xTextFromRight = props.entryPadding.right;
                // If relevant, take into account the padding.
                if(maxXSpawnArrow != 0.0f)
                {
                    xArrowIco += props.textArrowPadding;
                    xTextFromRight += maxXSpawnArrow + props.textArrowPadding;
                }

                float xWidth = xArrowIco + maxXSpawnArrow + props.splitterPadding.right;

                // Do the alignment of content.
                xWidth += props.entryPadding.right;
                atleastOne = false;
                float finalMenuWidth = xWidth + props.outerPadding.left + props.outerPadding.right;
                for (int i = 0; i < childrenCreationCache.Count; ++i)
                {
                    NodeCreationCache ccache = childrenCreationCache[i];
                    Node n = ccache.node;

                    if(atleastOne == false)
                        atleastOne = true;
                    else
                        fY += props.childrenSpacing;

                    float height = 
                        ccache.height - 
                        props.entryPadding.top - 
                        props.entryPadding.bottom;

                    if (n.type == Node.Type.Action || n.type == Node.Type.Menu || n.type == Node.Type.GoBack)
                    {
                        ccache.plate.rectTransform.anchoredPosition = 
                            new Vector2(
                                props.outerPadding.left, 
                                -fY);

                        ccache.plate.rectTransform.sizeDelta = 
                            new Vector2(
                                xWidth, ccache.height);

                        ccache.text.rectTransform.offsetMin = new Vector2(xTextStart, props.entryPadding.bottom);
                        ccache.text.rectTransform.offsetMax = new Vector2(-xTextFromRight, -props.entryPadding.top);

                        if(ccache.icon != null)
                        { 
                            ccache.icon.rectTransform.anchoredPosition = 
                                new Vector2(
                                    xIcoStart + (maxXIco - ccache.icon.sprite.rect.width) * 0.5f,
                                    -props.entryPadding.top - (height - ccache.icon.sprite.rect.height) * 0.5f);
                        }

                        if(ccache.arrow != null)
                        {
                            ccache.arrow.rectTransform.anchoredPosition = 
                                new Vector2(
                                    xArrowIco ,
                                    -props.entryPadding.top - (height - ccache.arrow.sprite.rect.height) * 0.5f);
                        }

                        fY += ccache.height;
                    }
                    else if(n.type == Node.Type.Separator)
                    {
                        float leftPad = props.outerPadding.left + props.splitterPadding.left;
                        float rightPad = props.outerPadding.right + props.splitterPadding.right;

                        ccache.plate.rectTransform.anchoredPosition = 
                            new Vector2( leftPad, -fY - props.splitterPadding.top );

                        ccache.plate.rectTransform.sizeDelta = 
                            new Vector2(
                                finalMenuWidth - leftPad - rightPad, 
                                props.splitter.rect.height );

                        fY += ccache.height;
                    }
                }

                fY += props.outerPadding.bottom;

                rtMenu.sizeDelta =
                    new Vector2(
                       finalMenuWidth, fY);

                rtMenu.anchoredPosition = Vector2.zero;
                rtMenu.position = topLeftSpawn;

                //      CHECK IF SCROLLMODE IS NEEDED IF MENU TALLER THAN CANVAS
                ////////////////////////////////////////////////////////////////////////////////

                Vector3 [] menuCorners = new Vector3[4];
                Vector3 [] canvasCorners = new Vector3[4];

                rtMenu.GetWorldCorners(menuCorners);
                this.modalPlate.GetWorldCorners(canvasCorners);

                // If the menu is taller than the canvas, we go into scroll mode
                float menuHeight = menuCorners[1].y - menuCorners[0].y;
                float canvasHeight = canvasCorners[1].y - canvasCorners[0].y;
                if ( menuHeight > canvasHeight)
                { 
                    List<Transform> menuChildren = new List<Transform>();
                    foreach(Transform t in rtMenu)
                        menuChildren.Add(t);

                    nctxRet.hasScroll = true;

                    float height = rtMenu.sizeDelta.y;

                    rtMenu.position = 
                        new Vector2(
                            menuCorners[0].x, 
                            canvasCorners[1].y);

                    rtMenu.sizeDelta = 
                        new Vector2(
                            rtMenu.sizeDelta.x + props.scrollbarWidth, 
                            this.modalPlate.rect.height);

                    GameObject goVP = new GameObject("Viewport");
                    goVP.transform.SetParent(rtMenu.transform, false);
                    UnityEngine.UI.Image imgVP = goVP.AddComponent<UnityEngine.UI.Image>();
                    UnityEngine.UI.Mask mask = goVP.AddComponent<UnityEngine.UI.Mask>();
                    mask.showMaskGraphic = false;
                    RectTransform rtView = imgVP.rectTransform;
                    rtView.anchorMin = new Vector2(0.0f, 0.0f);
                    rtView.anchorMax = new Vector2(1.0f, 1.0f);
                    rtView.offsetMin = new Vector2(0.0f, 0.0f);
                    rtView.offsetMax = new Vector2(-props.scrollbarWidth, 0.0f);

                    GameObject goVScroll = new GameObject("VScroll");
                    goVScroll.transform.SetParent(rtMenu, false);
                    UnityEngine.UI.Image imgVScroll = goVScroll.AddComponent<UnityEngine.UI.Image>();
                    imgVScroll.sprite = props.scrollbarPlate;
                    imgVScroll.type = UnityEngine.UI.Image.Type.Sliced;
                    RectTransform rtVScroll = imgVScroll.rectTransform;
                    rtVScroll.anchorMin = new Vector2(1.0f, 0.0f);
                    rtVScroll.anchorMax = new Vector2(1.0f, 1.0f);
                    rtVScroll.pivot = new Vector2(1.0f, 1.0f);
                    rtVScroll.sizeDelta = new Vector2(props.scrollbarWidth, 0.0f);

                    GameObject goVSThumb = new GameObject("VSThumb");
                    goVSThumb.transform.SetParent(goVScroll.transform, false);
                    UnityEngine.UI.Image imgVSThumb = goVSThumb.AddComponent<UnityEngine.UI.Image>();
                    RectTransform rtVSThumb = imgVSThumb.rectTransform;
                    rtVSThumb.sizeDelta = Vector2.zero;

                    UnityEngine.UI.Scrollbar scrollVert = goVScroll.AddComponent<UnityEngine.UI.Scrollbar>();
                    scrollVert.targetGraphic = imgVSThumb;
                    props.overflowScrollbar.Apply(scrollVert, imgVSThumb);
                    scrollVert.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
                    scrollVert.handleRect = rtVSThumb;

                    GameObject goCont = new GameObject("Content");
                    goCont.transform.SetParent(goVP.transform, false);
                    RectTransform rtCont = goCont.AddComponent<RectTransform>();
                    rtCont.pivot = new Vector2(0.0f, 1.0f);
                    rtCont.anchorMin = new Vector2(0.0f, 1.0f);
                    rtCont.anchorMax = new Vector2(1.0f, 1.0f);
                    rtCont.sizeDelta = new Vector2(0.0f, height);

                    // Move the Menu entries over
                    foreach(Transform t in menuChildren)
                        t.SetParent(goCont.transform, false);

                    UnityEngine.UI.ScrollRect scrt = rtMenu.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
                    scrt.verticalScrollbar = scrollVert;
                    scrt.viewport = rtView;
                    scrt.content = rtCont;
                    scrt.gameObject.SetActive(false);   // Touch to make dirty and refresh itself
                    scrt.gameObject.SetActive(true);
                    scrt.scrollSensitivity = props.scrollSensitivity;
                    //
                    scrt.onValueChanged.AddListener( 
                        (x)=>
                        { 
                            this.PopMenu(node);
                        });

                }
                // Else if the menu falls off the bottom of the screen, bring it back up
                else if(menuCorners[0].y < canvasCorners[0].y)
                { 
                    // There's no-doubt more optimized ways to calculate this, especially when
                    // constrainted to be axis aligned.
                    Vector3 locconv = new Vector3(0.0f, canvasCorners[0].y - menuCorners[0].y, 0.0f);
                    Vector3 wconv = rtMenu.worldToLocalMatrix.MultiplyVector(locconv);

                    rtMenu.anchoredPosition = 
                        new Vector2(
                            rtMenu.anchoredPosition.x,
                            rtMenu.anchoredPosition.y + wconv.y);
                }


                ////////////////////////////////////////////////////////////////////////////////
                // Move menu to top - menu is now top, and shadow is now 2nd top.
                rtMenu.SetAsLastSibling();

                // Some things might move the menu around later and re-LayoutShaow().
                // The chances of is happening multiple times is fine and the overhead is neglible
                Transform baseForShadowZ = null;
                if(this.spawnedSubmenus.Count >= 2)
                    baseForShadowZ = this.spawnedSubmenus[this.spawnedSubmenus.Count - 2].plate.transform;
                //
                nctxRet.LayoutShadow(this.spawner.props, baseForShadowZ);

                return nctxRet;
            }
        }
    }
}