using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExerciseParamProvider
{
    ExerciseEnumParam paramNumber {get; }
    ExerciseEnumParam paramTimed {get; }
    ExerciseEnumParam paramShowAnswers {get; }
    ExerciseEnumParam paramIntervalLabel {get; }
    ExerciseEnumParam paramBreak {get; }
    ExerciseEnumParam paramDirection {get; }
}