// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Editor.Host;
using Microsoft.CodeAnalysis.Editor.Implementation.FindReferences;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.FindReferences
{
    internal class DefinitionsAndReferencesPresenter : IDefinitionsAndReferencesPresenter
    {
        public DefinitionsAndReferences DefinitionsAndReferences;

        public void DisplayResult(DefinitionsAndReferences definitionsAndReferences)
        {
            DefinitionsAndReferences = definitionsAndReferences;
        }
    }
}