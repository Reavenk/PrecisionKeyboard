using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseContent
{
    public enum ContentType
    {
        Document,
        KeyboardQuiz,
        EarTraining
    }

    public enum ContentIcon
    {
        Notepad,
        Keyboard,
        Quiz
    }

    /// <summary>
    /// The cached version of the application.
    /// </summary>
    protected Application app;

    /// <summary>
    /// The exercise title.
    /// </summary>
    public abstract string title { get; }

    /// <summary>
    /// A description of the exercise.
    /// </summary>
    public abstract string description { get; }

    public virtual string longDescription { get { return this.description; } }

    public virtual bool Initialize(Application app, ExerciseAssets assets, IExerciseParamProvider sharedParams)
    {
        this.app = app;
        return true;
    }

    public abstract PxPre.UIL.Dialog CreateDialog(ExerciseAssets assets);

    public abstract ContentType GetExerciseType();

    public virtual ContentIcon GetExerciseIcon()
    {
        switch (this.GetExerciseType())
        {
            case ContentType.Document:
                return ContentIcon.Notepad;

            case ContentType.EarTraining:
                return ContentIcon.Quiz;

            default:
            case ContentType.KeyboardQuiz:
                return ContentIcon.Keyboard;
        }
    }
}
