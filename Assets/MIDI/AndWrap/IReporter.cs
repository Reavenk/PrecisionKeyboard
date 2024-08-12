// <copyright file="IReporter.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Message reporting class for the PxPreAndWrap class.</summary>


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    { 
        public interface IReporter
        {
            void ReportLog(string log);
            void ReportWarning(string log);
            void ReportError(string log);
            void ReportException(string log);
        }
    }
}