using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommentUIUpdater : ParamUIUpdater
{
    GNUIHost hostToUpdate;
    LLDNComment comment;
    UnityEngine.UI.Text commentText;
    RectTransform commentContainer;

    public CommentUIUpdater(
        GNUIHost hostToUpdate,
        LLDNComment comment,
        UnityEngine.UI.Text commentText,
        RectTransform rtContainer):
        base(rtContainer)
    { 
        this.hostToUpdate = hostToUpdate;
        this.comment = comment;
        this.commentText = commentText;
        this.commentContainer = rtContainer;
    }

    public override void Update(
        WiringDocument owningDoc, 
        WiringCollection collection)
    { 
        this.hostToUpdate.UpdateDimensionsFromComment(
            this.commentText,
            this.commentContainer,
            this.comment);
    }
}
