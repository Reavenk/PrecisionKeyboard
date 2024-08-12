// Decompiled with JetBrains decompiler
// Type: UnityEngine.UI.Scrollbar
// Assembly: UnityEngine.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2216A18B-AF52-44A5-85A0-A1CAA19C1090
// Assembly location: C:\Users\Blake\sandbox\unity\test-project\Library\UnityAssemblies\UnityEngine.UI.dll

using System;
using System.Collections;
using System.Diagnostics;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /// <summary>
    ///   <para>A standard scrollbar with a variable sized handle that can be dragged between 0 and 1.</para>
    /// </summary>
    [RequireComponent(typeof (RectTransform))]
    public class MyScrollbar : Selectable, IEventSystemHandler, IBeginDragHandler, IInitializePotentialDragHandler, IDragHandler, ICanvasElement
    {
          [SerializeField]
          [Range(0.0f, 1f)]
          private float m_Size = 0.2f;
          [Space(6f)]
          [SerializeField]
          private MyScrollbar.ScrollEvent m_OnValueChanged = new MyScrollbar.ScrollEvent();
          private Vector2 m_Offset = Vector2.zero;
          [SerializeField]
          private RectTransform m_HandleRect;
          [SerializeField]
          private MyScrollbar.Direction m_Direction;
          [SerializeField]
          [Range(0.0f, 1f)]
          private float m_Value;
          [SerializeField]
          [Range(0.0f, 11f)]
          private int m_NumberOfSteps;
          private RectTransform m_ContainerRect;
          private DrivenRectTransformTracker m_Tracker;
          private Coroutine m_PointerDownRepeat;
          private bool isPointerDownAndNotDragging;

          /// <summary>
          ///   <para>The RectTransform to use for the handle.</para>
          /// </summary>
          public RectTransform handleRect
          {
                get
                {
                    return this.m_HandleRect;
                }
                set
                {
                    if (!MySetPropertyUtility.SetClass<RectTransform>(ref this.m_HandleRect, value))
                        return;

                    this.UpdateCachedReferences();
                    this.UpdateVisuals();
                }
          }

          /// <summary>
          ///   <para>The direction of the scrollbar from minimum to maximum value.</para>
          /// </summary>
          public MyScrollbar.Direction direction
          {
                get
                {
                    return this.m_Direction;
                }
                set
                {
                    if (!MySetPropertyUtility.SetStruct<MyScrollbar.Direction>(ref this.m_Direction, value))
                        return;

                    this.UpdateVisuals();
                }
          }

          /// <summary>
          ///   <para>The current value of the scrollbar, between 0 and 1.</para>
          /// </summary>
          public float value
          {
              get
              {
                  float num = this.m_Value;
                  if (this.m_NumberOfSteps > 1)
                      num = Mathf.Round(num * (float) (this.m_NumberOfSteps - 1)) / (float) (this.m_NumberOfSteps - 1);

                  return num;
              }
              set
              {
                  this.Set(value);
              }
          }

          /// <summary>
          ///   <para>The size of the scrollbar handle where 1 means it fills the entire scrollbar.</para>
          /// </summary>
          public float size
          {
              get
              {
                  return this.m_Size;
              }
              set
              {
                  if (!MySetPropertyUtility.SetStruct<float>(ref this.m_Size, Mathf.Clamp01(value)))
                      return;

                  this.UpdateVisuals();
              }
          }

          /// <summary>
          ///   <para>The number of steps to use for the value. A value of 0 disables use of steps.</para>
          /// </summary>
          public int numberOfSteps
          {
              get
              {
                  return this.m_NumberOfSteps;
              }
              set
              {
                  if (!MySetPropertyUtility.SetStruct<int>(ref this.m_NumberOfSteps, value))
                      return;

                  this.Set(this.m_Value);
                  this.UpdateVisuals();
              }
          }

          /// <summary>
          ///   <para>Handling for when the scrollbar value is changed.</para>
          /// </summary>
          public MyScrollbar.ScrollEvent onValueChanged
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

          private float stepSize
          {
              get
              {
                  if (this.m_NumberOfSteps > 1)
                      return 1f / (float) (this.m_NumberOfSteps - 1);

                  return 0.1f;
              }
          }

          private MyScrollbar.Axis axis
          {
              get
              {
                  return this.m_Direction == MyScrollbar.Direction.LeftToRight || this.m_Direction == MyScrollbar.Direction.RightToLeft ? MyScrollbar.Axis.Horizontal : MyScrollbar.Axis.Vertical;
              }
          }

          private bool reverseValue
          {
              get
              {
                  if (this.m_Direction != MyScrollbar.Direction.RightToLeft)
                      return this.m_Direction == MyScrollbar.Direction.TopToBottom;

                  return true;
              }
          }

          protected MyScrollbar()
          {
          }

          // Was part of the original decompiled version downloaded online,
          // but failed to compile on android if included.
          // (wleu 07/01/2020)
          //
          //protected override void OnValidate()
          //{
          //    base.OnValidate();
          //    this.m_Size = Mathf.Clamp01(this.m_Size);
          //    if (this.IsActive())
          //    {
          //        this.UpdateCachedReferences();
          //        this.Set(this.m_Value, false);
          //        this.UpdateVisuals();
          //    }
          //    if (PrefabUtility.GetPrefabType((UnityEngine.Object) this) == PrefabType.Prefab || Application.isPlaying)
          //        return;
          //
          //    CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild((ICanvasElement) this);
          //}

          /// <summary>
          ///   <para>Handling for when the canvas is rebuilt.</para>
          /// </summary>
          /// <param name="executing"></param>
          public virtual void Rebuild(CanvasUpdate executing)
          {
              if (executing != CanvasUpdate.Prelayout)
                  return;

              this.onValueChanged.Invoke(this.value);
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

          protected override void OnEnable()
          {
              base.OnEnable();
              this.UpdateCachedReferences();
              this.Set(this.m_Value, false);
              this.UpdateVisuals();
          }

          /// <summary>
          ///   <para>See MonoBehaviour.OnDisable.</para>
          /// </summary>
          protected override void OnDisable()
          {
              this.m_Tracker.Clear();
              base.OnDisable();
          }

          private void UpdateCachedReferences()
          {
              if (this.m_HandleRect != null && this.m_HandleRect.parent != null)
                  this.m_ContainerRect = this.m_HandleRect.parent.GetComponent<RectTransform>();
              else
                  this.m_ContainerRect = null;
          }

          private void Set(float input)
          {
              this.Set(input, true);
          }

          private void Set(float input, bool sendCallback)
          {
              float num = this.m_Value;
              this.m_Value = Mathf.Clamp01(input);

              if ((double) num == (double) this.value)
                  return;

              this.UpdateVisuals();

              if (!sendCallback)
                  return;

              this.m_OnValueChanged.Invoke(this.value);
          }

          protected override void OnRectTransformDimensionsChange()
          {
                  base.OnRectTransformDimensionsChange();

              if (!this.IsActive())
                  return;

              this.UpdateVisuals();
          }

          private void UpdateVisuals()
          {
                if (!Application.isPlaying)
                    this.UpdateCachedReferences();

                this.m_Tracker.Clear();

                if (!((UnityEngine.Object) this.m_ContainerRect != (UnityEngine.Object) null))
                    return;

                this.m_Tracker.Add((UnityEngine.Object) this, this.m_HandleRect, DrivenTransformProperties.Anchors);
                Vector2 zero = Vector2.zero;
                Vector2 one = Vector2.one;
                float num = this.value * (1f - this.size);
                if (this.reverseValue)
                {
                    zero[(int) this.axis] = 1f - num - this.size;
                    one[(int) this.axis] = 1f - num;
                }
                else
                {
                    zero[(int) this.axis] = num;
                    one[(int) this.axis] = num + this.size;
                }
                this.m_HandleRect.anchorMin = zero;
                this.m_HandleRect.anchorMax = one;
          }

          private void UpdateDrag(PointerEventData eventData)
          {
                Vector2 localPoint;
                if (eventData.button != PointerEventData.InputButton.Left || (UnityEngine.Object) this.m_ContainerRect == (UnityEngine.Object) null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(this.m_ContainerRect, eventData.position, eventData.pressEventCamera, out localPoint))
                    return;

                Vector2 vector2 = localPoint - this.m_Offset - this.m_ContainerRect.rect.position - (this.m_HandleRect.rect.size - this.m_HandleRect.sizeDelta) * 0.5f;
                float num = (this.axis != MyScrollbar.Axis.Horizontal ? this.m_ContainerRect.rect.height : this.m_ContainerRect.rect.width) * (1f - this.size);
                if ((double) num <= 0.0)
                    return;

                switch (this.m_Direction)
                {
                    case MyScrollbar.Direction.LeftToRight:
                        this.Set(vector2.x / num);
                        break;
                    case MyScrollbar.Direction.RightToLeft:
                        this.Set((float) (1.0 - (double) vector2.x / (double) num));
                        break;
                    case MyScrollbar.Direction.BottomToTop:
                        this.Set(vector2.y / num);
                        break;
                    case MyScrollbar.Direction.TopToBottom:
                        this.Set((float) (1.0 - (double) vector2.y / (double) num));
                        break;
                }
          }

          private bool MayDrag(PointerEventData eventData)
          {
                if (this.IsActive() && this.IsInteractable())
                    return eventData.button == PointerEventData.InputButton.Left;

                return false;
          }

          /// <summary>
          ///   <para>Handling for when the scrollbar value is begin being dragged.</para>
          /// </summary>
          /// <param name="eventData"></param>
          public virtual void OnBeginDrag(PointerEventData eventData)
          {
                this.isPointerDownAndNotDragging = false;
                if (!this.MayDrag(eventData) || (UnityEngine.Object) this.m_ContainerRect == (UnityEngine.Object) null)
                    return;

                this.m_Offset = Vector2.zero;
                Vector2 localPoint;
                if (!RectTransformUtility.RectangleContainsScreenPoint(this.m_HandleRect, eventData.position, eventData.enterEventCamera) || !RectTransformUtility.ScreenPointToLocalPointInRectangle(this.m_HandleRect, eventData.position, eventData.pressEventCamera, out localPoint))
                    return;

                this.m_Offset = localPoint - this.m_HandleRect.rect.center;
          }

          /// <summary>
          ///   <para>Handling for when the scrollbar value is dragged.</para>
          /// </summary>
          /// <param name="eventData"></param>
          public virtual void OnDrag(PointerEventData eventData)
          {
                if (!this.MayDrag(eventData) || !((UnityEngine.Object) this.m_ContainerRect != (UnityEngine.Object) null))
                    return;

                this.UpdateDrag(eventData);
          }

          public override void OnPointerDown(PointerEventData eventData)
          {
                if (!this.MayDrag(eventData))
                    return;

                base.OnPointerDown(eventData);
                this.isPointerDownAndNotDragging = true;
                //this.m_PointerDownRepeat = this.StartCoroutine(this.ClickRepeat(eventData));

                if (!RectTransformUtility.RectangleContainsScreenPoint(m_HandleRect, eventData.position, eventData.enterEventCamera))
                { 
                    // There's more that can be done here, but this is specifically for 
                    // the MultitouchScrollbar - which is then specifically for the keyboard
                    // scrollbar so we're going to do the minimum here ... 
                    // - horizontal scroll
                    // - left to right.
                    //
                    // (wleu 07/01/2020)

                    RectTransform thisrt = this.gameObject.GetComponent<RectTransform>();

                    
                    Vector2 szCont = thisrt.rect.size;
                    Vector2 szHandle = this.m_HandleRect.rect.size;
                    Vector2 pos = thisrt.worldToLocalMatrix.MultiplyPoint(eventData.position);

                    // If it's less than half the thumb handle's with away, move it by how far it is 
                    // off the edge, up to half the handle's distance. That way if we just want to scoot
                    // near the edge instead of center where we clicked, we can.
                    float halfHandleWidth = szHandle.x / 2.0f;

                    float curMid = Mathf.Lerp(halfHandleWidth, szCont.x - halfHandleWidth, this.value);
                    if(pos.x < curMid)
                    { 
                        float leftEdge = Mathf.Lerp(0, szCont.x - szHandle.x, this.value);
                        if(leftEdge - pos.x < halfHandleWidth)
                            pos.x = curMid - (leftEdge - pos.x) * 2.0f;
                    }
                    else
                    { 
                        float rightEdge = Mathf.Lerp(szHandle.x, szCont.x, this.value);
                        if( pos.x - rightEdge < halfHandleWidth)
                            pos.x = curMid + (pos.x - rightEdge) * 2.0f;
                    }

                    float val = Mathf.InverseLerp(halfHandleWidth, szCont.x - halfHandleWidth, pos.x);

                    this.value = val;

                }
          }

          /// <summary>
          /// Coroutine function for handling continual press during Scrollbar.OnPointerDown.
          /// </summary>
          protected IEnumerator ClickRepeat(PointerEventData eventData)
          {
              while (isPointerDownAndNotDragging)
              {
                  if (!RectTransformUtility.RectangleContainsScreenPoint(m_HandleRect, eventData.position, eventData.enterEventCamera))
                  {
                      Vector2 localMousePos;
                      if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.position, eventData.pressEventCamera, out localMousePos))
                      {
                          var axisCoordinate = axis == 0 ? localMousePos.x : localMousePos.y;

                          // modifying value depending on direction, fixes (case 925824)

                          float change = axisCoordinate < 0 ? size : -size;
                          value += reverseValue ? change : -change;
                      }
                  }
                  yield return new WaitForEndOfFrame();
              }
              StopCoroutine(m_PointerDownRepeat);
          }

          public override void OnPointerUp(PointerEventData eventData)
          {
                base.OnPointerUp(eventData);
                this.isPointerDownAndNotDragging = false;
          }

          /// <summary>
          ///   <para>Handling for movement events.</para>
          /// </summary>
          /// <param name="eventData"></param>
          public override void OnMove(AxisEventData eventData)
          {
                if (!this.IsActive() || !this.IsInteractable())
                {
                    base.OnMove(eventData);
                }
                else
                {
                    switch (eventData.moveDir)
                    {
                    case MoveDirection.Left:
                        if (this.axis == MyScrollbar.Axis.Horizontal && (UnityEngine.Object) this.FindSelectableOnLeft() == (UnityEngine.Object) null)
                        {
                            this.Set(!this.reverseValue ? this.value - this.stepSize : this.value + this.stepSize);
                            break;
                        }
                        base.OnMove(eventData);
                        break;
                        
                    case MoveDirection.Up:
                        if (this.axis == MyScrollbar.Axis.Vertical && (UnityEngine.Object) this.FindSelectableOnUp() == (UnityEngine.Object) null)
                        {
                            this.Set(!this.reverseValue ? this.value + this.stepSize : this.value - this.stepSize);
                            break;
                        }
                        base.OnMove(eventData);
                        break;
                        
                    case MoveDirection.Right:
                        if (this.axis == MyScrollbar.Axis.Horizontal && (UnityEngine.Object) this.FindSelectableOnRight() == (UnityEngine.Object) null)
                        {
                            this.Set(!this.reverseValue ? this.value + this.stepSize : this.value - this.stepSize);
                            break;
                        }
                        base.OnMove(eventData);
                        break;

                    case MoveDirection.Down:
                          if (this.axis == MyScrollbar.Axis.Vertical && (UnityEngine.Object) this.FindSelectableOnDown() == (UnityEngine.Object) null)
                          {
                                this.Set(!this.reverseValue ? this.value - this.stepSize : this.value + this.stepSize);
                                break;
                          }
                          base.OnMove(eventData);
                          break;
                    }
                }
          }

          /// <summary>
          ///   <para>See member in base class.</para>
          /// </summary>
          public override Selectable FindSelectableOnLeft()
          {
                if (this.navigation.mode == Navigation.Mode.Automatic && this.axis == MyScrollbar.Axis.Horizontal)
                    return (Selectable) null;

                return base.FindSelectableOnLeft();
          }

          /// <summary>
          ///   <para>See member in base class.</para>
          /// </summary>
          public override Selectable FindSelectableOnRight()
          {
                if (this.navigation.mode == Navigation.Mode.Automatic && this.axis == MyScrollbar.Axis.Horizontal)
                    return (Selectable) null;

                return base.FindSelectableOnRight();
          }

          /// <summary>
          ///   <para>See member in base class.</para>
          /// </summary>
          public override Selectable FindSelectableOnUp()
          {
                if (this.navigation.mode == Navigation.Mode.Automatic && this.axis == MyScrollbar.Axis.Vertical)
                    return (Selectable) null;

                return base.FindSelectableOnUp();
          }

        /// <summary>
        ///   <para>See member in base class.</para>
        /// </summary>
        public override Selectable FindSelectableOnDown()
        {
            if (this.navigation.mode == Navigation.Mode.Automatic && this.axis == MyScrollbar.Axis.Vertical)
                return (Selectable) null;

            return base.FindSelectableOnDown();
        }

        /// <summary>
        ///   <para>See: IInitializePotentialDragHandler.OnInitializePotentialDrag.</para>
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        public void SetDirection(MyScrollbar.Direction direction, bool includeRectLayouts)
        {
            MyScrollbar.Axis axis = this.axis;
            bool reverseValue = this.reverseValue;
            this.direction = direction;
            if (!includeRectLayouts)
                return;

            if (this.axis != axis)
                RectTransformUtility.FlipLayoutAxes(this.transform as RectTransform, true, true);

            if (this.reverseValue == reverseValue)
                return;

            RectTransformUtility.FlipLayoutOnAxis(this.transform as RectTransform, (int) this.axis, true, true);
        }

        bool ICanvasElement.IsDestroyed()
        {
            return this.IsDestroyed();
        }


        // Has issues, probably related to the decompiling process
        // (wleu 07/01/2020)
        //
        //Transform ICanvasElement.get_transform()
        //{
        //  return this.transform;
        //}

        /// <summary>
        ///   <para>Setting that indicates one of four directions.</para>
        /// </summary>
        public enum Direction
        {
            LeftToRight,
            RightToLeft,
            BottomToTop,
            TopToBottom,
        }

        /// <summary>
        ///   <para>UnityEvent callback for when a scrollbar is scrolled.</para>
        /// </summary>
        [Serializable]
        public class ScrollEvent : UnityEvent<float>
        {
        }

        private enum Axis
        {
            Horizontal,
            Vertical,
        }
    }
}