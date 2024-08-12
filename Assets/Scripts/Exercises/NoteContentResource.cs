using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteContentResource : TextContent
{
    public ContentType contentType = ContentType.Document;
    public readonly string resourcePath;

    public NoteContentResource(string title, string resourcePath, string description, bool fullscreen)
        : base(title, description, fullscreen)
    { 
        this.resourcePath = resourcePath;
    }

    public override string GetContent()
    { 
        TextAsset tl = Resources.Load<TextAsset>(resourcePath);
        if(tl == null || string.IsNullOrEmpty(tl.text))
            return "Missing document content.";

        return tl.text;
    }

    public override ContentType GetExerciseType()
    {
        return this.contentType;
    }
}
