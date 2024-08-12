using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGNParamUICreator
{
    Font StandardFont {get; }

    ParamUIUpdater CreateFloatRangeSlider(RectTransform rt, ref float y, LLDNBase genNode, ParamFloat param);
    ParamUIUpdater CreateIntRangeSlider(RectTransform rt, ref float y, LLDNBase genNode, ParamInt param);

    ParamUIUpdater CreateFloatTime(RectTransform rt, ref float y, LLDNBase genNode, ParamFloat param);
    ParamUIUpdater CreateFloatClampedDial(RectTransform rt, ref float y, LLDNBase genNode, ParamFloat param);
    ParamUIUpdater CreateEnumPulldown(RectTransform rt, ref float y, LLDNBase genNode, ParamEnum param);
    ParamUIUpdater CreateBoolSwitch(RectTransform rt, ref float y, LLDNBase genNode, ParamBool param);
    ParamUIUpdater CreateTimeLenEdit(RectTransform rt, ref float y, LLDNBase genNode, ParamTimeLen param);
    ParamUIUpdater CreateWiringReference(RectTransform rt, ref float y, LLDNBase genNode, ParamWireReference param);
    ParamUIUpdater CreateNickname(RectTransform rt, ref float y, LLDNBase genNode, ParamNickname param);
    UnityEngine.UI.Image CreatePCMConnection(RectTransform rt, ref float y, LLDNBase genNode, ParamConnection param);

    void ConnectToOutput(LLDNQuickOut quickOutToConnect);

    float AccumulateHeight(ParamBase.Type type, string widgetName, ref float y);
    void PerformAction(RectTransform invoker, string actionname);
    Sprite GetIcon(string name);
}
