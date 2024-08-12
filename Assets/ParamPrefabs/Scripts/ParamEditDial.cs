using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ParamEditDial : ParamEditBase
{
    ParamFloat paramFloat = null;
    float origFloat = 0.0f;
    string origStringValue = string.Empty;

    public UnityEngine.UI.InputField input;
    public RectTransform needle;

    public RectTransform dialBody;

    public MaskableGUICircle hitRegion;

    public override bool SetParam(ParamFloat pf) 
    { 
        this.paramFloat = pf;
        this.origFloat = pf.value;
        this.origStringValue = pf.GetStringValue();
        this.UpdateInput();
        this.UpdateDial();
        return true; 
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        Vector2 local = dialBody.worldToLocalMatrix.MultiplyPoint(eventData.position);
        if (local.magnitude > this.hitRegion.radius)
            return;


        eventData.useDragThreshold = false;
        this.UpdateFromPointerEvent(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        this.UpdateFromPointerEvent(eventData);
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        Vector2 local = dialBody.worldToLocalMatrix.MultiplyPoint(eventData.position);
        if (local.magnitude > this.hitRegion.radius)
            eventData.pointerDrag = null;
    }

    public void UpdateFromPointerEvent(PointerEventData eventData)
    {
        Vector2 local = dialBody.worldToLocalMatrix.MultiplyPoint(eventData.position);

        float th = 10.0f / 8 * Mathf.PI - Mathf.Atan2(local.y, local.x);
        th = th % (Mathf.PI * 2.0f);

        float lam = th / (Mathf.PI * 2.0f * (3.0f / 4.0f));

        if (lam > 1.0f)
        {
            if (local.x <= 0.0f)
                lam = 0.0f;
            else
                lam = 1.0f;
        }


        paramFloat.value = lam;
        this.UpdateInput();
        this.UpdateDial();

        this.baseThumb.UpdateDisplayValue();
    }

    public void UpdateDial()
    {
        float val = 0.0f;

        if(this.paramFloat != null)
            val = this.paramFloat.value;

        const float minDialAngle = -135.0f;
        const float maxDialAngle = 135.0f;
        float degs = Mathf.Lerp(maxDialAngle, minDialAngle, val);
        needle.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, degs);
    }

    public void UpdateInput()
    { 
        if(this.paramFloat != null)
            input.text = this.paramFloat.value.ToString("0.000");
    }

    public void OnInput_ValueChanged()
    { 
        if(this.paramFloat != null)
        {
            float f;
            if(float.TryParse(this.input.text, out f) == true)
            {
                this.paramFloat.value = 
                    Mathf.Clamp( 
                        f, 
                        this.paramFloat.min,
                        this.paramFloat.max);

                this.UpdateDial();
                this.baseThumb.UpdateDisplayValue();
            }
        }
    }

    public void OnInput_EndEdit()
    { 
        bool processed = false;
        if(this.paramFloat != null)
        {
            float f;
            processed = float.TryParse(this.input.text, out f);
            if (processed == true)
            {
                this.paramFloat.value =
                    Mathf.Clamp(
                        f,
                        this.paramFloat.min,
                        this.paramFloat.max);

                this.UpdateDial();
                this.baseThumb.UpdateDisplayValue();
                this.UpdateInput();
            }
        }

        if(processed == true)
        { 
            if(Input.GetKeyDown(KeyCode.Return) == true)
                this.dlgRoot.CloseDialog();

        }
        else
            this.UpdateInput();
    }

    public override void OnConfirm()
    { 
        if(this.paramFloat != null)
        { 
            if(this.paramFloat.value != this.origFloat)
            {
                this.dlgRoot.application.SetLLDAWParamValue(
                    this.owner, 
                    this.paramFloat, 
                    this.origStringValue, 
                    this.paramFloat.GetStringValue());

                return;
            }
        }
    }
}
