// Taken from https://raw.githubusercontent.com/jamesjlinden/unity-decompiled/master/UnityEngine.UI/UI/ScrollRect.cs
// Modified to remedy an edge case with WiringScrollRect - when the scale was changed from needing 
// a scrollbar to not, on either axis, a jarring jump would occur.
// (wleu 07/01/2020)

// Decompiled with JetBrains decompiler
// Type: UnityEngine.UI.ScrollRect
// Assembly: UnityEngine.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2216A18B-AF52-44A5-85A0-A1CAA19C1090
// Assembly location: C:\Users\Blake\sandbox\unity\test-project\Library\UnityAssemblies\UnityEngine.UI.dll

using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
  /// <summary>
  ///   <para>A component for making a child RectTransform scroll.</para>
  /// </summary>
  [SelectionBase]
  [ExecuteInEditMode]
  [DisallowMultipleComponent]
  [RequireComponent(typeof (RectTransform))]
  public class MyScrollRect : UIBehaviour, IEventSystemHandler, IBeginDragHandler, IInitializePotentialDragHandler, IDragHandler, IEndDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutController, ILayoutGroup
  {
    [SerializeField]
    private bool m_Horizontal = true;
    [SerializeField]
    private bool m_Vertical = true;
    [SerializeField]
    private MyScrollRect.MovementType m_MovementType = MyScrollRect.MovementType.Elastic;
    [SerializeField]
    private float m_Elasticity = 0.1f;
    [SerializeField]
    private bool m_Inertia = true;
    [SerializeField]
    private float m_DecelerationRate = 0.135f;
    [SerializeField]
    private float m_ScrollSensitivity = 1f;
    [SerializeField]
    private ScrollRect.ScrollRectEvent m_OnValueChanged = new ScrollRect.ScrollRectEvent();
    private Vector2 m_PointerStartLocalCursor = Vector2.zero;
    private Vector2 m_ContentStartPosition = Vector2.zero;
    private Vector2 m_PrevPosition = Vector2.zero;
    private readonly Vector3[] m_Corners = new Vector3[4];
    [SerializeField]
    private RectTransform m_Content;
    [SerializeField]
    private RectTransform m_Viewport;
    [SerializeField]
    private Scrollbar m_HorizontalScrollbar;
    [SerializeField]
    private Scrollbar m_VerticalScrollbar;
    [SerializeField]
    private ScrollRect.ScrollbarVisibility m_HorizontalScrollbarVisibility;
    [SerializeField]
    private ScrollRect.ScrollbarVisibility m_VerticalScrollbarVisibility;
    [SerializeField]
    private float m_HorizontalScrollbarSpacing;
    [SerializeField]
    private float m_VerticalScrollbarSpacing;
    private RectTransform m_ViewRect;
    private Bounds m_ContentBounds;
    private Bounds m_ViewBounds;
    private Vector2 m_Velocity;
    private bool m_Dragging;
    private Bounds m_PrevContentBounds;
    private Bounds m_PrevViewBounds;
    [NonSerialized]
    private bool m_HasRebuiltLayout;
    private bool m_HSliderExpand;
    private bool m_VSliderExpand;
    private float m_HSliderHeight;
    private float m_VSliderWidth;
    [NonSerialized]
    private RectTransform m_Rect;
    private RectTransform m_HorizontalScrollbarRect;
    private RectTransform m_VerticalScrollbarRect;
    private DrivenRectTransformTracker m_Tracker;

    /// <summary>
    ///   <para>The content that can be scrolled. It should be a child of the GameObject with ScrollRect on it.</para>
    /// </summary>
    public RectTransform content
    {
      get
      {
        return this.m_Content;
      }
      set
      {
        this.m_Content = value;
      }
    }

    /// <summary>
    ///   <para>Should horizontal scrolling be enabled?</para>
    /// </summary>
    public bool horizontal
    {
      get
      {
        return this.m_Horizontal;
      }
      set
      {
        this.m_Horizontal = value;
      }
    }

    /// <summary>
    ///   <para>Should vertical scrolling be enabled?</para>
    /// </summary>
    public bool vertical
    {
      get
      {
        return this.m_Vertical;
      }
      set
      {
        this.m_Vertical = value;
      }
    }

    /// <summary>
    ///   <para>The behavior to use when the content moves beyond the scroll rect.</para>
    /// </summary>
    public MyScrollRect.MovementType movementType
    {
      get
      {
        return this.m_MovementType;
      }
      set
      {
        this.m_MovementType = value;
      }
    }

    /// <summary>
    ///   <para>The amount of elasticity to use when the content moves beyond the scroll rect.</para>
    /// </summary>
    public float elasticity
    {
      get
      {
        return this.m_Elasticity;
      }
      set
      {
        this.m_Elasticity = value;
      }
    }

    /// <summary>
    ///   <para>Should movement inertia be enabled?</para>
    /// </summary>
    public bool inertia
    {
      get
      {
        return this.m_Inertia;
      }
      set
      {
        this.m_Inertia = value;
      }
    }

    /// <summary>
    ///   <para>The rate at which movement slows down.</para>
    /// </summary>
    public float decelerationRate
    {
      get
      {
        return this.m_DecelerationRate;
      }
      set
      {
        this.m_DecelerationRate = value;
      }
    }

    /// <summary>
    ///   <para>The sensitivity to scroll wheel and track pad scroll events.</para>
    /// </summary>
    public float scrollSensitivity
    {
      get
      {
        return this.m_ScrollSensitivity;
      }
      set
      {
        this.m_ScrollSensitivity = value;
      }
    }

    /// <summary>
    ///   <para>Reference to the viewport RectTransform that is the parent of the content RectTransform.</para>
    /// </summary>
    public RectTransform viewport
    {
      get
      {
        return this.m_Viewport;
      }
      set
      {
        this.m_Viewport = value;
        this.SetDirtyCaching();
      }
    }

    /// <summary>
    ///   <para>Optional Scrollbar object linked to the horizontal scrolling of the ScrollRect.</para>
    /// </summary>
    public Scrollbar horizontalScrollbar
    {
      get
      {
        return this.m_HorizontalScrollbar;
      }
      set
      {
        if ((bool) ((UnityEngine.Object) this.m_HorizontalScrollbar))
          this.m_HorizontalScrollbar.onValueChanged.RemoveListener(new UnityAction<float>(this.SetHorizontalNormalizedPosition));
        this.m_HorizontalScrollbar = value;
        if ((bool) ((UnityEngine.Object) this.m_HorizontalScrollbar))
          this.m_HorizontalScrollbar.onValueChanged.AddListener(new UnityAction<float>(this.SetHorizontalNormalizedPosition));
        this.SetDirtyCaching();
      }
    }

    /// <summary>
    ///   <para>Optional Scrollbar object linked to the vertical scrolling of the ScrollRect.</para>
    /// </summary>
    public Scrollbar verticalScrollbar
    {
      get
      {
        return this.m_VerticalScrollbar;
      }
      set
      {
        if ((bool) ((UnityEngine.Object) this.m_VerticalScrollbar))
          this.m_VerticalScrollbar.onValueChanged.RemoveListener(new UnityAction<float>(this.SetVerticalNormalizedPosition));
        this.m_VerticalScrollbar = value;
        if ((bool) ((UnityEngine.Object) this.m_VerticalScrollbar))
          this.m_VerticalScrollbar.onValueChanged.AddListener(new UnityAction<float>(this.SetVerticalNormalizedPosition));
        this.SetDirtyCaching();
      }
    }

    /// <summary>
    ///   <para>The mode of visibility for the horizontal scrollbar.</para>
    /// </summary>
    public ScrollRect.ScrollbarVisibility horizontalScrollbarVisibility
    {
      get
      {
        return this.m_HorizontalScrollbarVisibility;
      }
      set
      {
        this.m_HorizontalScrollbarVisibility = value;
        this.SetDirtyCaching();
      }
    }

    /// <summary>
    ///   <para>The mode of visibility for the vertical scrollbar.</para>
    /// </summary>
    public ScrollRect.ScrollbarVisibility verticalScrollbarVisibility
    {
      get
      {
        return this.m_VerticalScrollbarVisibility;
      }
      set
      {
        this.m_VerticalScrollbarVisibility = value;
        this.SetDirtyCaching();
      }
    }

    /// <summary>
    ///   <para>The space between the scrollbar and the viewport.</para>
    /// </summary>
    public float horizontalScrollbarSpacing
    {
      get
      {
        return this.m_HorizontalScrollbarSpacing;
      }
      set
      {
        this.m_HorizontalScrollbarSpacing = value;
        this.SetDirty();
      }
    }

    /// <summary>
    ///   <para>The space between the scrollbar and the viewport.</para>
    /// </summary>
    public float verticalScrollbarSpacing
    {
      get
      {
        return this.m_VerticalScrollbarSpacing;
      }
      set
      {
        this.m_VerticalScrollbarSpacing = value;
        this.SetDirty();
      }
    }

    /// <summary>
    ///   <para>Callback executed when the scroll position of the slider is changed.</para>
    /// </summary>
    public ScrollRect.ScrollRectEvent onValueChanged
    {
      get
      {
        return this.m_OnValueChanged;
      }
      set
      {
        this.m_OnValueChanged = value;
      }
    }

    protected RectTransform viewRect
    {
      get
      {
        if ((UnityEngine.Object) this.m_ViewRect == (UnityEngine.Object) null)
          this.m_ViewRect = this.m_Viewport;
        if ((UnityEngine.Object) this.m_ViewRect == (UnityEngine.Object) null)
          this.m_ViewRect = (RectTransform) this.transform;
        return this.m_ViewRect;
      }
    }

    /// <summary>
    ///   <para>The current velocity of the content.</para>
    /// </summary>
    public Vector2 velocity
    {
      get
      {
        return this.m_Velocity;
      }
      set
      {
        this.m_Velocity = value;
      }
    }

    private RectTransform rectTransform
    {
      get
      {
        if ((UnityEngine.Object) this.m_Rect == (UnityEngine.Object) null)
          this.m_Rect = this.GetComponent<RectTransform>();
        return this.m_Rect;
      }
    }

    /// <summary>
    ///   <para>The scroll position as a Vector2 between (0,0) and (1,1) with (0,0) being the lower left corner.</para>
    /// </summary>
    public Vector2 normalizedPosition
    {
      get
      {
        return new Vector2(this.horizontalNormalizedPosition, this.verticalNormalizedPosition);
      }
      set
      {
        this.SetNormalizedPosition(value.x, 0);
        this.SetNormalizedPosition(value.y, 1);
      }
    }

    /// <summary>
    ///   <para>The horizontal scroll position as a value between 0 and 1, with 0 being at the left.</para>
    /// </summary>
    public float horizontalNormalizedPosition
    {
      get
      {
        this.UpdateBounds();
        if ((double) this.m_ContentBounds.size.x <= (double) this.m_ViewBounds.size.x)
          return (double) this.m_ViewBounds.min.x <= (double) this.m_ContentBounds.min.x ? 0.0f : 1f;
        return (float) (((double) this.m_ViewBounds.min.x - (double) this.m_ContentBounds.min.x) / ((double) this.m_ContentBounds.size.x - (double) this.m_ViewBounds.size.x));
      }
      set
      {
        this.SetNormalizedPosition(value, 0);
      }
    }

    /// <summary>
    ///   <para>The vertical scroll position as a value between 0 and 1, with 0 being at the bottom.</para>
    /// </summary>
    public float verticalNormalizedPosition
    {
      get
      {
        this.UpdateBounds();
        if ((double) this.m_ContentBounds.size.y <= (double) this.m_ViewBounds.size.y)
          return (double) this.m_ViewBounds.min.y <= (double) this.m_ContentBounds.min.y ? 0.0f : 1f;
        return (float) (((double) this.m_ViewBounds.min.y - (double) this.m_ContentBounds.min.y) / ((double) this.m_ContentBounds.size.y - (double) this.m_ViewBounds.size.y));
      }
      set
      {
        this.SetNormalizedPosition(value, 1);
      }
    }

    private bool hScrollingNeeded
    {
      get
      {
        if (Application.isPlaying)
          return (double) this.m_ContentBounds.size.x > (double) this.m_ViewBounds.size.x + 0.00999999977648258;
        return true;
      }
    }

    private bool vScrollingNeeded
    {
      get
      {
        if (Application.isPlaying)
          return (double) this.m_ContentBounds.size.y > (double) this.m_ViewBounds.size.y + 0.00999999977648258;
        return true;
      }
    }

    /// <summary>
    ///   <para>Called by the layout system.</para>
    /// </summary>
    public virtual float minWidth
    {
      get
      {
        return -1f;
      }
    }

    /// <summary>
    ///   <para>Called by the layout system.</para>
    /// </summary>
    public virtual float preferredWidth
    {
      get
      {
        return -1f;
      }
    }

    /// <summary>
    ///   <para>Called by the layout system.</para>
    /// </summary>
    public virtual float flexibleWidth
    {
      get
      {
        return -1f;
      }
    }

    /// <summary>
    ///   <para>Called by the layout system.</para>
    /// </summary>
    public virtual float minHeight
    {
      get
      {
        return -1f;
      }
    }

    /// <summary>
    ///   <para>Called by the layout system.</para>
    /// </summary>
    public virtual float preferredHeight
    {
      get
      {
        return -1f;
      }
    }

    /// <summary>
    ///   <para>Called by the layout system.</para>
    /// </summary>
    public virtual float flexibleHeight
    {
      get
      {
        return -1f;
      }
    }

    /// <summary>
    ///   <para>Called by the layout system.</para>
    /// </summary>
    public virtual int layoutPriority
    {
      get
      {
        return -1;
      }
    }

    protected MyScrollRect()
    {
    }

    /// <summary>
    ///   <para>Rebuilds the scroll rect data after initialization.</para>
    /// </summary>
    /// <param name="executing">The current step of the rendering CanvasUpdate cycle.</param>
    public virtual void Rebuild(CanvasUpdate executing)
    {
      if (executing == CanvasUpdate.Prelayout)
        this.UpdateCachedData();
      if (executing != CanvasUpdate.PostLayout)
        return;
      this.UpdateBounds();
      this.UpdateScrollbars(Vector2.zero);
      this.UpdatePrevData();
      this.m_HasRebuiltLayout = true;
    }

    /// <summary>
    ///   <para>See ICanvasElement.LayoutComplete.</para>
    /// </summary>
    public virtual void LayoutComplete()
    {
    }

    /// <summary>
    ///   <para>See ICanvasElement.GraphicUpdateComplete.</para>
    /// </summary>
    public virtual void GraphicUpdateComplete()
    {
    }

    private void UpdateCachedData()
    {
      Transform transform = this.transform;
      this.m_HorizontalScrollbarRect = !((UnityEngine.Object) this.m_HorizontalScrollbar == (UnityEngine.Object) null) ? this.m_HorizontalScrollbar.transform as RectTransform : (RectTransform) null;
      this.m_VerticalScrollbarRect = !((UnityEngine.Object) this.m_VerticalScrollbar == (UnityEngine.Object) null) ? this.m_VerticalScrollbar.transform as RectTransform : (RectTransform) null;
      bool flag = (UnityEngine.Object) this.viewRect.parent == (UnityEngine.Object) transform && (!(bool) ((UnityEngine.Object) this.m_HorizontalScrollbarRect) || (UnityEngine.Object) this.m_HorizontalScrollbarRect.parent == (UnityEngine.Object) transform) && (!(bool) ((UnityEngine.Object) this.m_VerticalScrollbarRect) || (UnityEngine.Object) this.m_VerticalScrollbarRect.parent == (UnityEngine.Object) transform);
      this.m_HSliderExpand = flag && (bool) ((UnityEngine.Object) this.m_HorizontalScrollbarRect) && this.horizontalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
      this.m_VSliderExpand = flag && (bool) ((UnityEngine.Object) this.m_VerticalScrollbarRect) && this.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
      this.m_HSliderHeight = !((UnityEngine.Object) this.m_HorizontalScrollbarRect == (UnityEngine.Object) null) ? this.m_HorizontalScrollbarRect.rect.height : 0.0f;
      this.m_VSliderWidth = !((UnityEngine.Object) this.m_VerticalScrollbarRect == (UnityEngine.Object) null) ? this.m_VerticalScrollbarRect.rect.width : 0.0f;
    }

    protected override void OnEnable()
    {
      base.OnEnable();
      if ((bool) ((UnityEngine.Object) this.m_HorizontalScrollbar))
        this.m_HorizontalScrollbar.onValueChanged.AddListener(new UnityAction<float>(this.SetHorizontalNormalizedPosition));
      if ((bool) ((UnityEngine.Object) this.m_VerticalScrollbar))
        this.m_VerticalScrollbar.onValueChanged.AddListener(new UnityAction<float>(this.SetVerticalNormalizedPosition));
      CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild((ICanvasElement) this);
    }

    /// <summary>
    ///   <para>See MonoBehaviour.OnDisable.</para>
    /// </summary>
    protected override void OnDisable()
    {
      CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild((ICanvasElement) this);
      if ((bool) ((UnityEngine.Object) this.m_HorizontalScrollbar))
        this.m_HorizontalScrollbar.onValueChanged.RemoveListener(new UnityAction<float>(this.SetHorizontalNormalizedPosition));
      if ((bool) ((UnityEngine.Object) this.m_VerticalScrollbar))
        this.m_VerticalScrollbar.onValueChanged.RemoveListener(new UnityAction<float>(this.SetVerticalNormalizedPosition));
      this.m_HasRebuiltLayout = false;
      this.m_Tracker.Clear();
      this.m_Velocity = Vector2.zero;
      LayoutRebuilder.MarkLayoutForRebuild(this.rectTransform);
      base.OnDisable();
    }

    /// <summary>
    ///   <para>See member in base class.</para>
    /// </summary>
    public override bool IsActive()
    {
      if (base.IsActive())
        return (UnityEngine.Object) this.m_Content != (UnityEngine.Object) null;
      return false;
    }

    private void EnsureLayoutHasRebuilt()
    {
      if (this.m_HasRebuiltLayout || CanvasUpdateRegistry.IsRebuildingLayout())
        return;
      Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    ///   <para>Sets the velocity to zero on both axes so the content stops moving.</para>
    /// </summary>
    public virtual void StopMovement()
    {
      this.m_Velocity = Vector2.zero;
    }

    /// <summary>
    ///   <para>See IScrollHandler.OnScroll.</para>
    /// </summary>
    /// <param name="data"></param>
    public virtual void OnScroll(PointerEventData data)
    {
      if (!this.IsActive())
        return;
      this.EnsureLayoutHasRebuilt();
      this.UpdateBounds();
      Vector2 scrollDelta = data.scrollDelta;
      scrollDelta.y *= -1f;
      if (this.vertical && !this.horizontal)
      {
        if ((double) Mathf.Abs(scrollDelta.x) > (double) Mathf.Abs(scrollDelta.y))
          scrollDelta.y = scrollDelta.x;
        scrollDelta.x = 0.0f;
      }
      if (this.horizontal && !this.vertical)
      {
        if ((double) Mathf.Abs(scrollDelta.y) > (double) Mathf.Abs(scrollDelta.x))
          scrollDelta.x = scrollDelta.y;
        scrollDelta.y = 0.0f;
      }
      Vector2 position = this.m_Content.anchoredPosition + scrollDelta * this.m_ScrollSensitivity;
      if (this.m_MovementType == MyScrollRect.MovementType.Clamped)
        position += this.CalculateOffset(position - this.m_Content.anchoredPosition);
      this.SetContentAnchoredPosition(position);
      this.UpdateBounds();
    }

    /// <summary>
    ///   <para>See: IInitializePotentialDragHandler.OnInitializePotentialDrag.</para>
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnInitializePotentialDrag(PointerEventData eventData)
    {
      if (eventData.button != PointerEventData.InputButton.Left)
        return;
      this.m_Velocity = Vector2.zero;
    }

    /// <summary>
    ///   <para>Handling for when the content is beging being dragged.</para>
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
      if (eventData.button != PointerEventData.InputButton.Left || !this.IsActive())
        return;
      this.UpdateBounds();
      this.m_PointerStartLocalCursor = Vector2.zero;
      RectTransformUtility.ScreenPointToLocalPointInRectangle(this.viewRect, eventData.position, eventData.pressEventCamera, out this.m_PointerStartLocalCursor);
      this.m_ContentStartPosition = this.m_Content.anchoredPosition;
      this.m_Dragging = true;
    }

    /// <summary>
    ///   <para>Handling for when the content has finished being dragged.</para>
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnEndDrag(PointerEventData eventData)
    {
      if (eventData.button != PointerEventData.InputButton.Left)
        return;
      this.m_Dragging = false;
    }

    /// <summary>
    ///   <para>Handling for when the content is dragged.</para>
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnDrag(PointerEventData eventData)
    {
      Vector2 localPoint;
      if (eventData.button != PointerEventData.InputButton.Left || !this.IsActive() || !RectTransformUtility.ScreenPointToLocalPointInRectangle(this.viewRect, eventData.position, eventData.pressEventCamera, out localPoint))
        return;
      this.UpdateBounds();
      Vector2 vector2 = this.m_ContentStartPosition + localPoint - this.m_PointerStartLocalCursor;
      Vector2 offset = this.CalculateOffset(vector2 - this.m_Content.anchoredPosition);
      Vector2 position = vector2 + offset;
      if (this.m_MovementType == MyScrollRect.MovementType.Elastic)
      {
        if ((double) offset.x != 0.0)
          position.x = position.x - MyScrollRect.RubberDelta(offset.x, this.m_ViewBounds.size.x);
        if ((double) offset.y != 0.0)
          position.y = position.y - MyScrollRect.RubberDelta(offset.y, this.m_ViewBounds.size.y);
      }
      this.SetContentAnchoredPosition(position);
    }

    /// <summary>
    ///   <para>Sets the anchored position of the content.</para>
    /// </summary>
    /// <param name="position"></param>
    protected virtual void SetContentAnchoredPosition(Vector2 position)
    {
      if (!this.m_Horizontal)
        position.x = this.m_Content.anchoredPosition.x;
      if (!this.m_Vertical)
        position.y = this.m_Content.anchoredPosition.y;
      if (!(position != this.m_Content.anchoredPosition))
        return;
      this.m_Content.anchoredPosition = position;
      this.UpdateBounds();
    }

    protected virtual void LateUpdate()
    {
        if (!(bool) ((UnityEngine.Object) this.m_Content))
            return;

        this.EnsureLayoutHasRebuilt();
        this.UpdateScrollbarVisibility();
        this.UpdateBounds();
        float unscaledDeltaTime = Time.unscaledDeltaTime;
        Vector2 offset = Vector2.zero;

        // Disabled the scrollbar auto-fixing itself if elastic so
        // WiringScrollRect doesn't get mucked with when doing
        // multi-touch translation.
        //
        // (wleu 07/01/2020)
        if(this.movementType != MovementType.Unrestricted)
            offset = this.CalculateOffset(Vector2.zero);

        if (!this.m_Dragging && (offset != Vector2.zero || this.m_Velocity != Vector2.zero))
        {
            Vector2 anchoredPosition = this.m_Content.anchoredPosition;
            for (int index = 0; index < 2; ++index)
            {
                if (this.m_MovementType == MyScrollRect.MovementType.Elastic && (double) offset[index] != 0.0)
                {
                    float currentVelocity = this.m_Velocity[index];
                    anchoredPosition[index] = Mathf.SmoothDamp(this.m_Content.anchoredPosition[index], this.m_Content.anchoredPosition[index] + offset[index], ref currentVelocity, this.m_Elasticity, float.PositiveInfinity, unscaledDeltaTime);
                    this.m_Velocity[index] = currentVelocity;
                }
                else if (this.m_Inertia)
                {
                    this.m_Velocity[index] *= Mathf.Pow(this.m_DecelerationRate, unscaledDeltaTime);
                    if ((double) Mathf.Abs(this.m_Velocity[index]) < 1.0)
                        this.m_Velocity[index] = 0.0f;
                    anchoredPosition[index] += this.m_Velocity[index] * unscaledDeltaTime;
                }
                else
                    this.m_Velocity[index] = 0.0f;
            }
            if (this.m_Velocity != Vector2.zero)
            {
                if (this.m_MovementType == MyScrollRect.MovementType.Clamped)
                {
                    offset = this.CalculateOffset(anchoredPosition - this.m_Content.anchoredPosition);
                    anchoredPosition += offset;
                }
                this.SetContentAnchoredPosition(anchoredPosition);
            }
        }
        if (this.m_Dragging && this.m_Inertia)
            this.m_Velocity = (Vector2) Vector3.Lerp((Vector3) this.m_Velocity, (Vector3) ((this.m_Content.anchoredPosition - this.m_PrevPosition) / unscaledDeltaTime), unscaledDeltaTime * 10f);

        if (!(this.m_ViewBounds != this.m_PrevViewBounds) && !(this.m_ContentBounds != this.m_PrevContentBounds) && !(this.m_Content.anchoredPosition != this.m_PrevPosition))
            return;

        this.UpdateScrollbars(offset);
        this.m_OnValueChanged.Invoke(this.normalizedPosition);
        this.UpdatePrevData();
    }

    private void UpdatePrevData()
    {
      this.m_PrevPosition = !((UnityEngine.Object) this.m_Content == (UnityEngine.Object) null) ? this.m_Content.anchoredPosition : Vector2.zero;
      this.m_PrevViewBounds = this.m_ViewBounds;
      this.m_PrevContentBounds = this.m_ContentBounds;
    }

    private void UpdateScrollbars(Vector2 offset)
    {
        if ((bool) ((UnityEngine.Object) this.m_HorizontalScrollbar))
        {
            //this.m_HorizontalScrollbar.size = (double) this.m_ContentBounds.size.x <= 0.0 ? 1f : Mathf.Clamp01((this.m_ViewBounds.size.x - Mathf.Abs(offset.x)) / this.m_ContentBounds.size.x);
            if((double) this.m_ContentBounds.size.x <= 0.0)
            { 
                this.m_HorizontalScrollbar.size = 1f;
            }
            else
            { 
                this.m_HorizontalScrollbar.size = 
                    Mathf.Clamp01((this.m_ViewBounds.size.x - Mathf.Abs(offset.x)) / this.m_ContentBounds.size.x);
            }

            this.m_HorizontalScrollbar.value = this.horizontalNormalizedPosition;
        }

        if (!(bool) ((UnityEngine.Object) this.m_VerticalScrollbar))
            return;

        //this.m_VerticalScrollbar.size = (double) this.m_ContentBounds.size.y <= 0.0 ? 1f : Mathf.Clamp01((this.m_ViewBounds.size.y - Mathf.Abs(offset.y)) / this.m_ContentBounds.size.y);
        if((double) this.m_ContentBounds.size.y <= 0.0)
        { 
            this.m_VerticalScrollbar.size = 1f;
        }
        else
        { 
            this.m_VerticalScrollbar.size = Mathf.Clamp01((this.m_ViewBounds.size.y - Mathf.Abs(offset.y)) / this.m_ContentBounds.size.y);
        }

      this.m_VerticalScrollbar.value = this.verticalNormalizedPosition;
    }

    private void SetHorizontalNormalizedPosition(float value)
    {
      this.SetNormalizedPosition(value, 0);
    }

    private void SetVerticalNormalizedPosition(float value)
    {
      this.SetNormalizedPosition(value, 1);
    }

    private void SetNormalizedPosition(float value, int axis)
    {
          this.EnsureLayoutHasRebuilt();
          this.UpdateBounds();
          float num1 = this.m_ContentBounds.size[axis] - this.m_ViewBounds.size[axis];
          float num2 = this.m_ViewBounds.min[axis] - value * num1;
          float num3 = this.m_Content.localPosition[axis] + num2 - this.m_ContentBounds.min[axis];
          Vector3 localPosition = this.m_Content.localPosition;
        if ((double) Mathf.Abs(localPosition[axis] - num3) <= 0.00999999977648258)
            return;

        // Disabled on restricted for use with WiringScrollRect during
        // pinch zoom.
        //
        // (wleu 07/01/2020)
        if(this.movementType != MovementType.Unrestricted)
        {
            localPosition[axis] = num3;
            this.m_Content.localPosition = localPosition;
        }
        this.m_Velocity[axis] = 0.0f;
        this.UpdateBounds();
    }

    private static float RubberDelta(float overStretching, float viewSize)
    {
      return (float) (1.0 - 1.0 / ((double) Mathf.Abs(overStretching) * 0.550000011920929 / (double) viewSize + 1.0)) * viewSize * Mathf.Sign(overStretching);
    }

    protected override void OnRectTransformDimensionsChange()
    {
      this.SetDirty();
    }

    /// <summary>
    ///   <para>Called by the layout system.</para>
    /// </summary>
    public virtual void CalculateLayoutInputHorizontal()
    {
    }

    /// <summary>
    ///   <para>Called by the layout system.</para>
    /// </summary>
    public virtual void CalculateLayoutInputVertical()
    {
    }

    /// <summary>
    ///   <para>Called by the layout system.</para>
    /// </summary>
    public virtual void SetLayoutHorizontal()
    {
      this.m_Tracker.Clear();
      if (this.m_HSliderExpand || this.m_VSliderExpand)
      {
        this.m_Tracker.Add((UnityEngine.Object) this, this.viewRect, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.SizeDelta);
        this.viewRect.anchorMin = Vector2.zero;
        this.viewRect.anchorMax = Vector2.one;
        this.viewRect.sizeDelta = Vector2.zero;
        this.viewRect.anchoredPosition = Vector2.zero;
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.content);
        this.m_ViewBounds = new Bounds((Vector3) this.viewRect.rect.center, (Vector3) this.viewRect.rect.size);
        this.m_ContentBounds = this.GetBounds();
      }
      if (this.m_VSliderExpand && this.vScrollingNeeded)
      {
        this.viewRect.sizeDelta = new Vector2((float) -((double) this.m_VSliderWidth + (double) this.m_VerticalScrollbarSpacing), this.viewRect.sizeDelta.y);
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.content);
        this.m_ViewBounds = new Bounds((Vector3) this.viewRect.rect.center, (Vector3) this.viewRect.rect.size);
        this.m_ContentBounds = this.GetBounds();
      }
      if (this.m_HSliderExpand && this.hScrollingNeeded)
      {
        this.viewRect.sizeDelta = new Vector2(this.viewRect.sizeDelta.x, (float) -((double) this.m_HSliderHeight + (double) this.m_HorizontalScrollbarSpacing));
        this.m_ViewBounds = new Bounds((Vector3) this.viewRect.rect.center, (Vector3) this.viewRect.rect.size);
        this.m_ContentBounds = this.GetBounds();
      }
      if (!this.m_VSliderExpand || !this.vScrollingNeeded || (double) this.viewRect.sizeDelta.x != 0.0 || (double) this.viewRect.sizeDelta.y >= 0.0)
        return;
      this.viewRect.sizeDelta = new Vector2((float) -((double) this.m_VSliderWidth + (double) this.m_VerticalScrollbarSpacing), this.viewRect.sizeDelta.y);
    }

    /// <summary>
    ///   <para>Called by the layout system.</para>
    /// </summary>
    public virtual void SetLayoutVertical()
    {
      this.UpdateScrollbarLayout();
      this.m_ViewBounds = new Bounds((Vector3) this.viewRect.rect.center, (Vector3) this.viewRect.rect.size);
      this.m_ContentBounds = this.GetBounds();
    }

    private void UpdateScrollbarVisibility()
    {
      if ((bool) ((UnityEngine.Object) this.m_VerticalScrollbar) && this.m_VerticalScrollbarVisibility != ScrollRect.ScrollbarVisibility.Permanent && this.m_VerticalScrollbar.gameObject.activeSelf != this.vScrollingNeeded)
        this.m_VerticalScrollbar.gameObject.SetActive(this.vScrollingNeeded);
      if (!(bool) ((UnityEngine.Object) this.m_HorizontalScrollbar) || this.m_HorizontalScrollbarVisibility == ScrollRect.ScrollbarVisibility.Permanent || this.m_HorizontalScrollbar.gameObject.activeSelf == this.hScrollingNeeded)
        return;
      this.m_HorizontalScrollbar.gameObject.SetActive(this.hScrollingNeeded);
    }

    private void UpdateScrollbarLayout()
    {
      if (this.m_VSliderExpand && (bool) ((UnityEngine.Object) this.m_HorizontalScrollbar))
      {
        this.m_Tracker.Add((UnityEngine.Object) this, this.m_HorizontalScrollbarRect, DrivenTransformProperties.AnchoredPositionX | DrivenTransformProperties.AnchorMinX | DrivenTransformProperties.AnchorMaxX | DrivenTransformProperties.SizeDeltaX);
        this.m_HorizontalScrollbarRect.anchorMin = new Vector2(0.0f, this.m_HorizontalScrollbarRect.anchorMin.y);
        this.m_HorizontalScrollbarRect.anchorMax = new Vector2(1f, this.m_HorizontalScrollbarRect.anchorMax.y);
        this.m_HorizontalScrollbarRect.anchoredPosition = new Vector2(0.0f, this.m_HorizontalScrollbarRect.anchoredPosition.y);
        this.m_HorizontalScrollbarRect.sizeDelta = !this.vScrollingNeeded ? new Vector2(0.0f, this.m_HorizontalScrollbarRect.sizeDelta.y) : new Vector2((float) -((double) this.m_VSliderWidth + (double) this.m_VerticalScrollbarSpacing), this.m_HorizontalScrollbarRect.sizeDelta.y);
      }
      if (!this.m_HSliderExpand || !(bool) ((UnityEngine.Object) this.m_VerticalScrollbar))
        return;
      this.m_Tracker.Add((UnityEngine.Object) this, this.m_VerticalScrollbarRect, DrivenTransformProperties.AnchoredPositionY | DrivenTransformProperties.AnchorMinY | DrivenTransformProperties.AnchorMaxY | DrivenTransformProperties.SizeDeltaY);
      this.m_VerticalScrollbarRect.anchorMin = new Vector2(this.m_VerticalScrollbarRect.anchorMin.x, 0.0f);
      this.m_VerticalScrollbarRect.anchorMax = new Vector2(this.m_VerticalScrollbarRect.anchorMax.x, 1f);
      this.m_VerticalScrollbarRect.anchoredPosition = new Vector2(this.m_VerticalScrollbarRect.anchoredPosition.x, 0.0f);
      if (this.hScrollingNeeded)
        this.m_VerticalScrollbarRect.sizeDelta = new Vector2(this.m_VerticalScrollbarRect.sizeDelta.x, (float) -((double) this.m_HSliderHeight + (double) this.m_HorizontalScrollbarSpacing));
      else
        this.m_VerticalScrollbarRect.sizeDelta = new Vector2(this.m_VerticalScrollbarRect.sizeDelta.x, 0.0f);
    }

    private void UpdateBounds()
    {
      this.m_ViewBounds = new Bounds((Vector3) this.viewRect.rect.center, (Vector3) this.viewRect.rect.size);
      this.m_ContentBounds = this.GetBounds();
      if ((UnityEngine.Object) this.m_Content == (UnityEngine.Object) null)
        return;
      Vector3 size = this.m_ContentBounds.size;
      Vector3 center = this.m_ContentBounds.center;
      Vector3 vector3 = this.m_ViewBounds.size - size;
      if ((double) vector3.x > 0.0)
      {
        center.x -= vector3.x * (this.m_Content.pivot.x - 0.5f);
        size.x = this.m_ViewBounds.size.x;
      }
      if ((double) vector3.y > 0.0)
      {
        center.y -= vector3.y * (this.m_Content.pivot.y - 0.5f);
        size.y = this.m_ViewBounds.size.y;
      }
      this.m_ContentBounds.size = size;
      this.m_ContentBounds.center = center;
    }

    private Bounds GetBounds()
    {
      if ((UnityEngine.Object) this.m_Content == (UnityEngine.Object) null)
        return new Bounds();
      Vector3 vector3_1 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
      Vector3 vector3_2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
      Matrix4x4 worldToLocalMatrix = this.viewRect.worldToLocalMatrix;
      this.m_Content.GetWorldCorners(this.m_Corners);
      for (int index = 0; index < 4; ++index)
      {
        Vector3 lhs = worldToLocalMatrix.MultiplyPoint3x4(this.m_Corners[index]);
        vector3_1 = Vector3.Min(lhs, vector3_1);
        vector3_2 = Vector3.Max(lhs, vector3_2);
      }
      Bounds bounds = new Bounds(vector3_1, Vector3.zero);
      bounds.Encapsulate(vector3_2);
      return bounds;
    }

    private Vector2 CalculateOffset(Vector2 delta)
    {
      Vector2 zero = Vector2.zero;
      if (this.m_MovementType == MyScrollRect.MovementType.Unrestricted)
        return zero;
      Vector2 min = (Vector2) this.m_ContentBounds.min;
      Vector2 max = (Vector2) this.m_ContentBounds.max;
      if (this.m_Horizontal)
      {
        min.x += delta.x;
        max.x += delta.x;
        if ((double) min.x > (double) this.m_ViewBounds.min.x)
          zero.x = this.m_ViewBounds.min.x - min.x;
        else if ((double) max.x < (double) this.m_ViewBounds.max.x)
          zero.x = this.m_ViewBounds.max.x - max.x;
      }
      if (this.m_Vertical)
      {
        min.y += delta.y;
        max.y += delta.y;
        if ((double) max.y < (double) this.m_ViewBounds.max.y)
          zero.y = this.m_ViewBounds.max.y - max.y;
        else if ((double) min.y > (double) this.m_ViewBounds.min.y)
          zero.y = this.m_ViewBounds.min.y - min.y;
      }
      return zero;
    }

    /// <summary>
    ///   <para>Override to alter or add to the code that keeps the appearance of the scroll rect synced with its data.</para>
    /// </summary>
    protected void SetDirty()
    {
      if (!this.IsActive())
        return;
      LayoutRebuilder.MarkLayoutForRebuild(this.rectTransform);
    }

    /// <summary>
    ///   <para>Override to alter or add to the code that caches data to avoid repeated heavy operations.</para>
    /// </summary>
    protected void SetDirtyCaching()
    {
      if (!this.IsActive())
        return;
      CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild((ICanvasElement) this);
      LayoutRebuilder.MarkLayoutForRebuild(this.rectTransform);
    }

    // Came with the decompile but wouldn't build for Android. Might be an old version,
    // or something that gets preprocessored but the preprocessor statement didn't get
    // transfered with the rest of the .cs?
    //
    // (wleu 07/02/2020)
    //protected override void OnValidate()
    //{
    //  this.SetDirtyCaching();
    //}

    bool ICanvasElement.IsDestroyed()
    {
      return this.IsDestroyed();
    }

    /// <summary>
    ///   <para>A setting for which behavior to use when content moves beyond the confines of its container.</para>
    /// </summary>
    public enum MovementType
    {
      Unrestricted,
      Elastic,
      Clamped,
    }

    /// <summary>
    ///   <para>Enum for which behavior to use for scrollbar visibility.</para>
    /// </summary>
    public enum ScrollbarVisibility
    {
      Permanent,
      AutoHide,
      AutoHideAndExpandViewport,
    }

    /// <summary>
    ///   <para>Event type used by the ScrollRect.</para>
    /// </summary>
    [Serializable]
    public class ScrollRectEvent : UnityEvent<Vector2>
    {
    }
  }
}
