// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.AddImport;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Diagnostics;
using Microsoft.CodeAnalysis.Options;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.AddUsing
{
    public partial class AddUsingTests : AbstractCSharpDiagnosticProviderBasedUserDiagnosticTest
    {
        internal override (DiagnosticAnalyzer, CodeFixProvider) CreateDiagnosticProviderAndFixer(Workspace workspace)
            => (null, new CSharpAddImportCodeFixProvider());

        private async Task TestAsync(
             string initialMarkup,
             string expected,
             bool systemSpecialCase,
             int index = 0,
             object fixProviderData = null)
        {
            await TestInRegularAndScriptAsync(initialMarkup, expected, index: index, fixProviderData: fixProviderData, options: new Dictionary<OptionKey, object>
            {
                { new OptionKey(GenerationOptions.PlaceSystemNamespaceFirst, LanguageNames.CSharp), systemSpecialCase }
            });
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestTypeFromMultipleNamespaces1()
        {
            await TestInRegularAndScriptAsync(
@"class Class
{
    [|IDictionary|] Method()
    {
        Foo();
    }
}",
@"using System.Collections;

class Class
{
    IDictionary Method()
    {
        Foo();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        [WorkItem(11241, "https://github.com/dotnet/roslyn/issues/11241")]
        public async Task TestAddImportWithCaseChange()
        {
            await TestInRegularAndScriptAsync(
@"namespace N1
{
    public class TextBox
    {
    }
}

class Class1 : [|Textbox|]
{
}",
@"using N1;

namespace N1
{
    public class TextBox
    {
    }
}

class Class1 : TextBox
{
}", priority: CodeActionPriority.Low);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestTypeFromMultipleNamespaces2()
        {
            await TestInRegularAndScriptAsync(
@"class Class
{
    [|IDictionary|] Method()
    {
        Foo();
    }
}",
@"using System.Collections.Generic;

class Class
{
    IDictionary Method()
    {
        Foo();
    }
}",
index: 1);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestGenericWithNoArgs()
        {
            await TestInRegularAndScriptAsync(
@"class Class
{
    [|List|] Method()
    {
        Foo();
    }
}",
@"using System.Collections.Generic;

class Class
{
    List Method()
    {
        Foo();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestGenericWithCorrectArgs()
        {
            await TestInRegularAndScriptAsync(
@"class Class
{
    [|List<int>|] Method()
    {
        Foo();
    }
}",
@"using System.Collections.Generic;

class Class
{
    List<int> Method()
    {
        Foo();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestGenericWithWrongArgs1()
        {
            await TestMissingInRegularAndScriptAsync(
@"class Class
{
    [|List<int, string, bool>|] Method()
    {
        Foo();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestGenericWithWrongArgs2()
        {
            await TestMissingInRegularAndScriptAsync(
@"class Class
{
    [|List<int, string>|] Method()
    {
        Foo();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestGenericInLocalDeclaration()
        {
            await TestInRegularAndScriptAsync(
@"class Class
{
    void Foo()
    {
        [|List<int>|] a = new List<int>();
    }
}",
@"using System.Collections.Generic;

class Class
{
    void Foo()
    {
        List<int> a = new List<int>();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestGenericItemType()
        {
            await TestInRegularAndScriptAsync(
@"using System.Collections.Generic;

class Class
{
    List<[|Int32|]> l;
}",
@"using System;
using System.Collections.Generic;

class Class
{
    List<Int32> l;
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestGenerateWithExistingUsings()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class Class
{
    [|List<int>|] Method()
    {
        Foo();
    }
}",
@"using System;
using System.Collections.Generic;

class Class
{
    List<int> Method()
    {
        Foo();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestGenerateInNamespace()
        {
            await TestInRegularAndScriptAsync(
@"namespace N
{
    class Class
    {
        [|List<int>|] Method()
        {
            Foo();
        }
    }
}",
@"using System.Collections.Generic;

namespace N
{
    class Class
    {
        List<int> Method()
        {
            Foo();
        }
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestGenerateInNamespaceWithUsings()
        {
            await TestInRegularAndScriptAsync(
@"namespace N
{
    using System;

    class Class
    {
        [|List<int>|] Method()
        {
            Foo();
        }
    }
}",
@"namespace N
{
    using System;
    using System.Collections.Generic;

    class Class
    {
        List<int> Method()
        {
            Foo();
        }
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestExistingUsing()
        {
            await TestActionCountAsync(
@"using System.Collections.Generic;

class Class
{
    [|IDictionary|] Method()
    {
        Foo();
    }
}",
count: 1);

            await TestInRegularAndScriptAsync(
@"using System.Collections.Generic;

class Class
{
    [|IDictionary|] Method()
    {
        Foo();
    }
}",
@"using System.Collections;
using System.Collections.Generic;

class Class
{
    IDictionary Method()
    {
        Foo();
    }
}");
        }

        [WorkItem(541730, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541730")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingForGenericExtensionMethod()
        {
            await TestInRegularAndScriptAsync(
@"using System.Collections.Generic;

class Class
{
    void Method(IList<int> args)
    {
        args.[|Where|]() }
}",
@"using System.Collections.Generic;
using System.Linq;

class Class
{
    void Method(IList<int> args)
    {
        args.Where() }
}");
        }

        [WorkItem(541730, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541730")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingForNormalExtensionMethod()
        {
            await TestAsync(
@"class Class
{
    void Method(Class args)
    {
        args.[|Where|]() }
}

namespace N
{
    static class E
    {
        public static void Where(this Class c)
        {
        }
    }
}",
@"using N;

class Class
{
    void Method(Class args)
    {
        args.Where() }
}

namespace N
{
    static class E
    {
        public static void Where(this Class c)
        {
        }
    }
}",
parseOptions: Options.Regular);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestOnEnum()
        {
            await TestInRegularAndScriptAsync(
@"class Class
{
    void Foo()
    {
        var a = [|Colors|].Red;
    }
}

namespace A
{
    enum Colors
    {
        Red,
        Green,
        Blue
    }
}",
@"using A;

class Class
{
    void Foo()
    {
        var a = Colors.Red;
    }
}

namespace A
{
    enum Colors
    {
        Red,
        Green,
        Blue
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestOnClassInheritance()
        {
            await TestInRegularAndScriptAsync(
@"class Class : [|Class2|]
{
}

namespace A
{
    class Class2
    {
    }
}",
@"using A;

class Class : Class2
{
}

namespace A
{
    class Class2
    {
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestOnImplementedInterface()
        {
            await TestInRegularAndScriptAsync(
@"class Class : [|IFoo|]
{
}

namespace A
{
    interface IFoo
    {
    }
}",
@"using A;

class Class : IFoo
{
}

namespace A
{
    interface IFoo
    {
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAllInBaseList()
        {
            await TestInRegularAndScriptAsync(
@"class Class : [|IFoo|], Class2
{
}

namespace A
{
    class Class2
    {
    }
}

namespace B
{
    interface IFoo
    {
    }
}",
@"using B;

class Class : IFoo, Class2
{
}

namespace A
{
    class Class2
    {
    }
}

namespace B
{
    interface IFoo
    {
    }
}");

            await TestInRegularAndScriptAsync(
@"using B;

class Class : IFoo, [|Class2|]
{
}

namespace A
{
    class Class2
    {
    }
}

namespace B
{
    interface IFoo
    {
    }
}",
@"using A;
using B;

class Class : IFoo, Class2
{
}

namespace A
{
    class Class2
    {
    }
}

namespace B
{
    interface IFoo
    {
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAttributeUnexpanded()
        {
            await TestInRegularAndScriptAsync(
@"[[|Obsolete|]]
class Class
{
}",
@"using System;

[Obsolete]
class Class
{
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAttributeExpanded()
        {
            await TestInRegularAndScriptAsync(
@"[[|ObsoleteAttribute|]]
class Class
{
}",
@"using System;

[ObsoleteAttribute]
class Class
{
}");
        }

        [WorkItem(538018, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/538018")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAfterNew()
        {
            await TestInRegularAndScriptAsync(
@"class Class
{
    void Foo()
    {
        List<int> l;
        l = new [|List<int>|]();
    }
}",
@"using System.Collections.Generic;

class Class
{
    void Foo()
    {
        List<int> l;
        l = new List<int>();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestArgumentsInMethodCall()
        {
            await TestInRegularAndScriptAsync(
@"class Class
{
    void Test()
    {
        Console.WriteLine([|DateTime|].Today);
    }
}",
@"using System;

class Class
{
    void Test()
    {
        Console.WriteLine(DateTime.Today);
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestCallSiteArgs()
        {
            await TestInRegularAndScriptAsync(
@"class Class
{
    void Test([|DateTime|] dt)
    {
    }
}",
@"using System;

class Class
{
    void Test(DateTime dt)
    {
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestUsePartialClass()
        {
            await TestInRegularAndScriptAsync(
@"namespace A
{
    public class Class
    {
        [|PClass|] c;
    }
}

namespace B
{
    public partial class PClass
    {
    }
}",
@"using B;

namespace A
{
    public class Class
    {
        PClass c;
    }
}

namespace B
{
    public partial class PClass
    {
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestGenericClassInNestedNamespace()
        {
            await TestInRegularAndScriptAsync(
@"namespace A
{
    namespace B
    {
        class GenericClass<T>
        {
        }
    }
}

namespace C
{
    class Class
    {
        [|GenericClass<int>|] c;
    }
}",
@"using A.B;

namespace A
{
    namespace B
    {
        class GenericClass<T>
        {
        }
    }
}

namespace C
{
    class Class
    {
        GenericClass<int> c;
    }
}");
        }

        [WorkItem(541730, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541730")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestExtensionMethods()
        {
            await TestInRegularAndScriptAsync(
@"using System.Collections.Generic;

class Foo
{
    void Bar()
    {
        var values = new List<int>();
        values.[|Where|](i => i > 1);
    }
}",
@"using System.Collections.Generic;
using System.Linq;

class Foo
{
    void Bar()
    {
        var values = new List<int>();
        values.Where(i => i > 1);
    }
}");
        }

        [WorkItem(541730, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541730")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestQueryPatterns()
        {
            await TestInRegularAndScriptAsync(
@"using System.Collections.Generic;

class Foo
{
    void Bar()
    {
        var values = new List<int>();
        var q = [|from v in values
                where v > 1
                select v + 10|];
    }
}",
@"using System.Collections.Generic;
using System.Linq;

class Foo
{
    void Bar()
    {
        var values = new List<int>();
        var q = from v in values
                where v > 1
                select v + 10;
    }
}");
        }

        // Tests for Insertion Order
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestSimplePresortedUsings1()
        {
            await TestInRegularAndScriptAsync(
@"using B;
using C;

class Class
{
    void Method()
    {
        [|Foo|].Bar();
    }
}

namespace D
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
@"using B;
using C;
using D;

class Class
{
    void Method()
    {
        Foo.Bar();
    }
}

namespace D
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestSimplePresortedUsings2()
        {
            await TestInRegularAndScriptAsync(
@"using B;
using C;

class Class
{
    void Method()
    {
        [|Foo|].Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
@"using A;
using B;
using C;

class Class
{
    void Method()
    {
        Foo.Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestSimpleUnsortedUsings1()
        {
            await TestInRegularAndScriptAsync(
@"using C;
using B;

class Class
{
    void Method()
    {
        [|Foo|].Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
@"using C;
using B;
using A;

class Class
{
    void Method()
    {
        Foo.Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestSimpleUnsortedUsings2()
        {
            await TestInRegularAndScriptAsync(
@"using D;
using B;

class Class
{
    void Method()
    {
        [|Foo|].Bar();
    }
}

namespace C
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
@"using D;
using B;
using C;

class Class
{
    void Method()
    {
        Foo.Bar();
    }
}

namespace C
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestMultiplePresortedUsings1()
        {
            await TestInRegularAndScriptAsync(
@"using B.X;
using B.Y;

class Class
{
    void Method()
    {
        [|Foo|].Bar();
    }
}

namespace B
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
@"using B;
using B.X;
using B.Y;

class Class
{
    void Method()
    {
        Foo.Bar();
    }
}

namespace B
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestMultiplePresortedUsings2()
        {
            await TestInRegularAndScriptAsync(
@"using B.X;
using B.Y;

class Class
{
    void Method()
    {
        [|Foo|].Bar();
    }
}

namespace B.A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
@"using B.A;
using B.X;
using B.Y;

class Class
{
    void Method()
    {
        Foo.Bar();
    }
}

namespace B.A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestMultiplePresortedUsings3()
        {
            await TestInRegularAndScriptAsync(
@"using B.X;
using B.Y;

class Class
{
    void Method()
    {
        [|Foo|].Bar();
    }
}

namespace B
{
    namespace A
    {
        class Foo
        {
            public static void Bar()
            {
            }
        }
    }
}",
@"using B.A;
using B.X;
using B.Y;

class Class
{
    void Method()
    {
        Foo.Bar();
    }
}

namespace B
{
    namespace A
    {
        class Foo
        {
            public static void Bar()
            {
            }
        }
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestMultipleUnsortedUsings1()
        {
            await TestInRegularAndScriptAsync(
@"using B.Y;
using B.X;

class Class
{
    void Method()
    {
        [|Foo|].Bar();
    }
}

namespace B
{
    namespace A
    {
        class Foo
        {
            public static void Bar()
            {
            }
        }
    }
}",
@"using B.Y;
using B.X;
using B.A;

class Class
{
    void Method()
    {
        Foo.Bar();
    }
}

namespace B
{
    namespace A
    {
        class Foo
        {
            public static void Bar()
            {
            }
        }
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestMultipleUnsortedUsings2()
        {
            await TestInRegularAndScriptAsync(
@"using B.Y;
using B.X;

class Class
{
    void Method()
    {
        [|Foo|].Bar();
    }
}

namespace B
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
@"using B.Y;
using B.X;
using B;

class Class
{
    void Method()
    {
        Foo.Bar();
    }
}

namespace B
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}");
        }

        // System on top cases
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestSimpleSystemSortedUsings1()
        {
            await TestAsync(
@"using System;
using B;

class Class
{
    void Method()
    {
        [|Foo|].Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
@"using System;
using A;
using B;

class Class
{
    void Method()
    {
        Foo.Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
systemSpecialCase: true);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestSimpleSystemSortedUsings2()
        {
            await TestAsync(
@"using System;
using System.Collections.Generic;
using B;

class Class
{
    void Method()
    {
        [|Foo|].Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
@"using System;
using System.Collections.Generic;
using A;
using B;

class Class
{
    void Method()
    {
        Foo.Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
systemSpecialCase: true);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestSimpleSystemSortedUsings3()
        {
            await TestAsync(
@"using A;
using B;

class Class
{
    void Method()
    {
        [|Console|].Write(1);
    }
}",
@"using System;
using A;
using B;

class Class
{
    void Method()
    {
        Console.Write(1);
    }
}",
systemSpecialCase: true);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestSimpleSystemUnsortedUsings1()
        {
            await TestAsync(
@"using B;
using System;

class Class
{
    void Method()
    {
        [|Foo|].Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
@"using B;
using System;
using A;

class Class
{
    void Method()
    {
        Foo.Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
systemSpecialCase: true);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestSimpleSystemUnsortedUsings2()
        {
            await TestAsync(
@"using System.Collections.Generic;
using System;
using B;

class Class
{
    void Method()
    {
        [|Foo|].Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
@"using System.Collections.Generic;
using System;
using B;
using A;

class Class
{
    void Method()
    {
        Foo.Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
systemSpecialCase: true);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestSimpleSystemUnsortedUsings3()
        {
            await TestAsync(
@"using B;
using A;

class Class
{
    void Method()
    {
        [|Console|].Write(1);
    }
}",
@"using B;
using A;
using System;

class Class
{
    void Method()
    {
        Console.Write(1);
    }
}",
systemSpecialCase: true);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestSimpleBogusSystemUsings1()
        {
            await TestAsync(
@"using A.System;

class Class
{
    void Method()
    {
        [|Console|].Write(1);
    }
}",
@"using System;
using A.System;

class Class
{
    void Method()
    {
        Console.Write(1);
    }
}",
systemSpecialCase: true);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestSimpleBogusSystemUsings2()
        {
            await TestAsync(
@"using System.System;

class Class
{
    void Method()
    {
        [|Console|].Write(1);
    }
}",
@"using System;
using System.System;

class Class
{
    void Method()
    {
        Console.Write(1);
    }
}",
systemSpecialCase: true);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestUsingsWithComments()
        {
            await TestAsync(
@"using System./*...*/.Collections.Generic;

class Class
{
    void Method()
    {
        [|Console|].Write(1);
    }
}",
@"using System;
using System./*...*/.Collections.Generic;

class Class
{
    void Method()
    {
        Console.Write(1);
    }
}",
systemSpecialCase: true);
        }

        // System Not on top cases
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestSimpleSystemUnsortedUsings4()
        {
            await TestAsync(
@"using System;
using B;

class Class
{
    void Method()
    {
        [|Foo|].Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
@"using System;
using B;
using A;

class Class
{
    void Method()
    {
        Foo.Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
systemSpecialCase: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestSimpleSystemSortedUsings5()
        {
            await TestAsync(
@"using B;
using System;

class Class
{
    void Method()
    {
        [|Foo|].Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
@"using A;
using B;
using System;

class Class
{
    void Method()
    {
        Foo.Bar();
    }
}

namespace A
{
    class Foo
    {
        public static void Bar()
        {
        }
    }
}",
systemSpecialCase: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestSimpleSystemSortedUsings4()
        {
            await TestAsync(
@"using A;
using B;

class Class
{
    void Method()
    {
        [|Console|].Write(1);
    }
}",
@"using A;
using B;
using System;

class Class
{
    void Method()
    {
        Console.Write(1);
    }
}",
systemSpecialCase: false);
        }

        [WorkItem(538136, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/538136")]
        [WorkItem(538763, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/538763")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingForNamespace()
        {
            await TestMissingInRegularAndScriptAsync(
@"namespace A
{
    class Class
    {
        [|C|].Test t;
    }
}

namespace B
{
    namespace C
    {
        class Test
        {
        }
    }
}");
        }

        [WorkItem(538220, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/538220")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingForFieldWithFormatting()
        {
            await TestInRegularAndScriptAsync(
@"class C { [|DateTime|] t; }",
@"using System;

class C { DateTime t; }",
ignoreTrivia: false);
        }

        [WorkItem(539657, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539657")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task BugFix5688()
        {
            await TestInRegularAndScriptAsync(
@"class Program { static void Main ( string [ ] args ) { [|Console|] . Out . NewLine = ""\r\n\r\n"" ; } } ",
@"using System ; class Program { static void Main ( string [ ] args ) { Console . Out . NewLine = ""\r\n\r\n"" ; } } ");
        }

        [WorkItem(539853, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539853")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task BugFix5950()
        {
            await TestAsync(
@"using System.Console; WriteLine([|Expression|].Constant(123));",
@"using System.Console;
using System.Linq.Expressions; WriteLine(Expression.Constant(123));",
parseOptions: GetScriptOptions());
        }

        [WorkItem(540339, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/540339")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddAfterDefineDirective1()
        {
            await TestInRegularAndScriptAsync(
@"#define foo

using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        [|Console|].WriteLine();
    }
}",
@"#define foo

using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine();
    }
}",
ignoreTrivia: false);
        }

        [WorkItem(540339, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/540339")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddAfterDefineDirective2()
        {
            await TestInRegularAndScriptAsync(
@"#define foo

class Program
{
    static void Main(string[] args)
    {
        [|Console|].WriteLine();
    }
}",
@"#define foo

using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine();
    }
}",
ignoreTrivia: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddAfterDefineDirective3()
        {
            await TestInRegularAndScriptAsync(
@"#define foo

/// Foo
class Program
{
    static void Main(string[] args)
    {
        [|Console|].WriteLine();
    }
}",
@"#define foo

using System;
/// Foo
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine();
    }
}",
ignoreTrivia: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddAfterDefineDirective4()
        {
            await TestInRegularAndScriptAsync(
@"#define foo

// Foo
class Program
{
    static void Main(string[] args)
    {
        [|Console|].WriteLine();
    }
}",
@"#define foo

// Foo
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine();
    }
}",
ignoreTrivia: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddAfterExistingBanner()
        {
            await TestInRegularAndScriptAsync(
@"// Banner
// Banner

class Program
{
    static void Main(string[] args)
    {
        [|Console|].WriteLine();
    }
}",
@"// Banner
// Banner

using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine();
    }
}",
ignoreTrivia: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddAfterExternAlias1()
        {
            await TestInRegularAndScriptAsync(
@"#define foo

extern alias Foo;

class Program
{
    static void Main(string[] args)
    {
        [|Console|].WriteLine();
    }
}",
@"#define foo

extern alias Foo;

using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine();
    }
}",
ignoreTrivia: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddAfterExternAlias2()
        {
            await TestInRegularAndScriptAsync(
@"#define foo

extern alias Foo;

using System.Collections;

class Program
{
    static void Main(string[] args)
    {
        [|Console|].WriteLine();
    }
}",
@"#define foo

extern alias Foo;

using System;
using System.Collections;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine();
    }
}",
ignoreTrivia: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestWithReferenceDirective()
        {
            var resolver = new TestMetadataReferenceResolver(assemblyNames: new Dictionary<string, PortableExecutableReference>()
            {
                { "exprs", AssemblyMetadata.CreateFromImage(TestResources.NetFX.v4_0_30319.System_Core).GetReference() }
            });

            await TestAsync(
@"#r ""exprs""
[|Expression|]",
@"#r ""exprs""
using System.Linq.Expressions;

Expression",
GetScriptOptions(),
TestOptions.ReleaseDll.WithMetadataReferenceResolver(resolver),
ignoreTrivia: false);
        }

        [WorkItem(542643, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542643")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAssemblyAttribute()
        {
            await TestInRegularAndScriptAsync(
@"[assembly: [|InternalsVisibleTo|](""Project"")]",
@"using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(""Project"")]");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestDoNotAddIntoHiddenRegion()
        {
            await TestMissingInRegularAndScriptAsync(
@"#line hidden
using System.Collections.Generic;
#line default

class Program
{
    void Main()
    {
        [|DateTime|] d;
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddToVisibleRegion()
        {
            await TestInRegularAndScriptAsync(
@"#line default
using System.Collections.Generic;

#line hidden
class Program
{
    void Main()
    {
#line default
        [|DateTime|] d;
#line hidden
    }
}
#line default",
@"#line default
using System;
using System.Collections.Generic;

#line hidden
class Program
{
    void Main()
    {
#line default
        DateTime d;
#line hidden
    }
}
#line default",
ignoreTrivia: false);
        }

        [WorkItem(545248, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545248")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestVenusGeneration1()
        {
            await TestMissingInRegularAndScriptAsync(
@"class C
{
    void Foo()
    {
#line 1 ""Default.aspx""
        using (new [|StreamReader|]())
        {
#line default
#line hidden
        }
    }");
        }

        [WorkItem(545774, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545774")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAttribute()
        {
            var input = @"[ assembly : [|Guid|] ( ""9ed54f84-a89d-4fcd-a854-44251e925f09"" ) ] ";
            await TestActionCountAsync(input, 1);

            await TestInRegularAndScriptAsync(
input,
@"using System.Runtime.InteropServices;

[assembly: Guid(""9ed54f84-a89d-4fcd-a854-44251e925f09"")]");
        }

        [WorkItem(546833, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/546833")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestNotOnOverloadResolutionError()
        {
            await TestMissingInRegularAndScriptAsync(
@"namespace ConsoleApplication1
{
    class Program
    {
        void Main()
        {
            var test = new [|Test|]("""");
        }
    }

    class Test
    {
    }
}");
        }

        [WorkItem(17020, "DevDiv_Projects/Roslyn")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingForGenericArgument()
        {
            await TestInRegularAndScriptAsync(
@"namespace ConsoleApplication10
{
    class Program
    {
        static void Main(string[] args)
        {
            var inArgument = new InArgument<[|IEnumerable<int>|]>(new int[] { 1, 2, 3 });
        }
    }

    public class InArgument<T>
    {
        public InArgument(T constValue)
        {
        }
    }
}",
@"using System.Collections.Generic;

namespace ConsoleApplication10
{
    class Program
    {
        static void Main(string[] args)
        {
            var inArgument = new InArgument<IEnumerable<int>>(new int[] { 1, 2, 3 });
        }
    }

    public class InArgument<T>
    {
        public InArgument(T constValue)
        {
        }
    }
}");
        }

        [WorkItem(775448, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/775448")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task ShouldTriggerOnCS0308()
        {
            // CS0308: The non-generic type 'A' cannot be used with type arguments
            await TestInRegularAndScriptAsync(
@"using System.Collections;

class Test
{
    static void Main(string[] args)
    {
        [|IEnumerable<int>|] f;
    }
}",
@"using System.Collections;
using System.Collections.Generic;

class Test
{
    static void Main(string[] args)
    {
        IEnumerable<int> f;
    }
}");
        }

        [WorkItem(838253, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/838253")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestConflictedInaccessibleType()
        {
            await TestAsync(
@"using System.Diagnostics;

namespace N
{
    public class Log
    {
    }
}

class C
{
    static void Main(string[] args)
    {
        [|Log|] }
}",
@"using System.Diagnostics;
using N;

namespace N
{
    public class Log
    {
    }
}

class C
{
    static void Main(string[] args)
    {
        Log }
}",
systemSpecialCase: true);
        }

        [WorkItem(858085, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/858085")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestConflictedAttributeName()
        {
            await TestInRegularAndScriptAsync(
@"[[|Description|]]
class Description
{
}",
@"using System.ComponentModel;

[Description]
class Description
{
}");
        }

        [WorkItem(872908, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/872908")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestConflictedGenericName()
        {
            await TestInRegularAndScriptAsync(
@"using Task = System.AccessViolationException;

class X
{
    [|Task<X> x;|]
}",
@"using System.Threading.Tasks;
using Task = System.AccessViolationException;

class X
{
    Task<X> x;
}");
        }

        [WorkItem(913300, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/913300")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestNoDuplicateReport()
        {
            await TestActionCountInAllFixesAsync(
@"class C
{
    void M(P p)
    {
        [|Console|]
    }

    static void Main(string[] args)
    {
    }
}", count: 1);

            await TestInRegularAndScriptAsync(
@"class C
{
    void M(P p)
    {
        [|Console|] }

    static void Main(string[] args)
    {
    }
}",
@"using System;

class C
{
    void M(P p)
    {
        Console }

    static void Main(string[] args)
    {
    }
}");
        }

        [WorkItem(938296, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/938296")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestNullParentInNode()
        {
            await TestMissingInRegularAndScriptAsync(
@"using System.Collections.Generic;

class MultiDictionary<K, V> : Dictionary<K, HashSet<V>>
{
    void M()
    {
        new HashSet<V>([|Comparer|]);
    }
}");
        }

        [WorkItem(968303, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/968303")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestMalformedUsingSection()
        {
            await TestMissingInRegularAndScriptAsync(
@"[ class Class
{
    [|List<|] }");
        }

        [WorkItem(875899, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/875899")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingsWithExternAlias()
        {
            const string InitialWorkspace = @"
<Workspace>
    <Project Language=""C#"" AssemblyName=""lib"" CommonReferences=""true"">
        <Document FilePath=""lib.cs"">
namespace ProjectLib
{
    public class Project
    {
    }
}
        </Document>
    </Project>
    <Project Language=""C#"" AssemblyName=""Console"" CommonReferences=""true"">
        <ProjectReference Alias=""P"">lib</ProjectReference>
        <Document FilePath=""Program.cs"">
namespace ExternAliases
{
    class Program
    {
        static void Main(string[] args)
        {
            Project p = new [|Project()|];
        }
    }
} 
</Document>
    </Project>
</Workspace>";

            const string ExpectedDocumentText = @"extern alias P;

using P::ProjectLib;
namespace ExternAliases
{
    class Program
    {
        static void Main(string[] args)
        {
            Project p = new Project();
        }
    }
} 
";
            await TestInRegularAndScriptAsync(InitialWorkspace, ExpectedDocumentText);
        }

        [WorkItem(875899, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/875899")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingsWithPreExistingExternAlias()
        {
            const string InitialWorkspace = @"
<Workspace>
    <Project Language=""C#"" AssemblyName=""lib"" CommonReferences=""true"">
        <Document FilePath=""lib.cs"">
namespace ProjectLib
{
    public class Project
    {
    }
}

namespace AnotherNS
{
    public class AnotherClass
    {
    }
}
        </Document>
    </Project>
    <Project Language=""C#"" AssemblyName=""Console"" CommonReferences=""true"">
        <ProjectReference Alias=""P"">lib</ProjectReference>
        <Document FilePath=""Program.cs"">
extern alias P;
using P::ProjectLib;
namespace ExternAliases
{
    class Program
    {
        static void Main(string[] args)
        {
            Project p = new Project();
            var x = new [|AnotherClass()|];
        }
    }
} 
</Document>
    </Project>
</Workspace>";

            const string ExpectedDocumentText = @"extern alias P;

using P::AnotherNS;
using P::ProjectLib;
namespace ExternAliases
{
    class Program
    {
        static void Main(string[] args)
        {
            Project p = new Project();
            var x = new [|AnotherClass()|];
        }
    }
} 
";
            await TestInRegularAndScriptAsync(InitialWorkspace, ExpectedDocumentText);
        }

        [WorkItem(875899, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/875899")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingsNoExtern()
        {
            const string InitialWorkspace = @"
<Workspace>
    <Project Language=""C#"" AssemblyName=""lib"" CommonReferences=""true"">
        <Document FilePath=""lib.cs"">
namespace AnotherNS
{
    public class AnotherClass
    {
    }
}
        </Document>
    </Project>
    <Project Language=""C#"" AssemblyName=""Console"" CommonReferences=""true"">
        <ProjectReference Alias=""P"">lib</ProjectReference>
        <Document FilePath=""Program.cs"">
using P::AnotherNS;
namespace ExternAliases
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = new [|AnotherClass()|];
        }
    }
} 
</Document>
    </Project>
</Workspace>";

            const string ExpectedDocumentText = @"extern alias P;
using P::AnotherNS;
namespace ExternAliases
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = new AnotherClass();
        }
    }
} 
";
            await TestInRegularAndScriptAsync(InitialWorkspace, ExpectedDocumentText);
        }

        [WorkItem(875899, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/875899")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingsNoExternFilterGlobalAlias()
        {
            await TestInRegularAndScriptAsync(
@"class Program
{
    static void Main(string[] args)
    {
        [|INotifyPropertyChanged.PropertyChanged|]
    }
}",
@"using System.ComponentModel;

class Program
{
    static void Main(string[] args)
    {
        INotifyPropertyChanged.PropertyChanged
    }
}");
        }

        [WorkItem(916368, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/916368")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingForCref()
        {
            var initialText =
@"/// <summary>
/// This is just like <see cref='[|INotifyPropertyChanged|]'/>, but this one is mine.
/// </summary>
interface MyNotifyPropertyChanged { }";

            var expectedText =
@"using System.ComponentModel;
/// <summary>
/// This is just like <see cref='INotifyPropertyChanged'/>, but this one is mine.
/// </summary>
interface MyNotifyPropertyChanged { }";

            var options = new CSharpParseOptions(documentationMode: DocumentationMode.Diagnose);

            await TestAsync(initialText, expectedText, parseOptions: options);
        }

        [WorkItem(916368, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/916368")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingForCref2()
        {
            var initialText =
@"/// <summary>
/// This is just like <see cref='[|INotifyPropertyChanged.PropertyChanged|]'/>, but this one is mine.
/// </summary>
interface MyNotifyPropertyChanged { }";

            var expectedText =
@"using System.ComponentModel;
/// <summary>
/// This is just like <see cref='INotifyPropertyChanged.PropertyChanged'/>, but this one is mine.
/// </summary>
interface MyNotifyPropertyChanged { }";

            var options = new CSharpParseOptions(documentationMode: DocumentationMode.Diagnose);

            await TestAsync(initialText, expectedText, parseOptions: options);
        }

        [WorkItem(916368, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/916368")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingForCref3()
        {
            var initialText =
@"namespace N1
{
    public class D { }
}

public class MyClass
{
    public static explicit operator N1.D (MyClass f)
    {
        return default(N1.D);
    }
}

/// <seealso cref='MyClass.explicit operator [|D(MyClass)|]'/>
public class MyClass2
{
}";

            var expectedText =
@"using N1;

namespace N1
{
    public class D { }
}

public class MyClass
{
    public static explicit operator N1.D(MyClass f)
    {
        return default(N1.D);
    }
}

/// <seealso cref='MyClass.explicit operator D(MyClass)'/>
public class MyClass2
{
}";

            var options = new CSharpParseOptions(documentationMode: DocumentationMode.Diagnose);

            await TestAsync(initialText, expectedText, parseOptions: options);
        }

        [WorkItem(916368, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/916368")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingForCref4()
        {
            var initialText =
@"namespace N1
{
    public class D { }
}

/// <seealso cref='[|Test(D)|]'/>
public class MyClass
{
    public void Test(N1.D i)
    {
    }
}";

            var expectedText =
@"using N1;

namespace N1
{
    public class D { }
}

/// <seealso cref='Test(D)'/>
public class MyClass
{
    public void Test(N1.D i)
    {
    }
}";

            var options = new CSharpParseOptions(documentationMode: DocumentationMode.Diagnose);

            await TestAsync(initialText, expectedText, parseOptions: options);
        }

        [WorkItem(773614, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/773614")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddStaticType()
        {
            var initialText =
@"using System;

public static class Outer
{
    [AttributeUsage(AttributeTargets.All)]
    public class MyAttribute : Attribute
    {

    }
}

[[|My|]]
class Test
{}";

            var expectedText =
@"using System;
using static Outer;

public static class Outer
{
    [AttributeUsage(AttributeTargets.All)]
    public class MyAttribute : Attribute
    {

    }
}

[My]
class Test
{}";

            await TestInRegularAndScriptAsync(initialText, expectedText);
        }

        [WorkItem(773614, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/773614")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddStaticType2()
        {
            var initialText =
@"using System;

public static class Outer
{
    public static class Inner
    {
        [AttributeUsage(AttributeTargets.All)]
        public class MyAttribute : Attribute
        {
        }
    }
}

[[|My|]]
class Test
{}";

            var expectedText =
@"using System;
using static Outer.Inner;

public static class Outer
{
    public static class Inner
    {
        [AttributeUsage(AttributeTargets.All)]
        public class MyAttribute : Attribute
        {
        }
    }
}

[My]
class Test
{}";

            await TestInRegularAndScriptAsync(initialText, expectedText);
        }

        [WorkItem(773614, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/773614")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddStaticType3()
        {
            await TestInRegularAndScriptAsync(
@"using System;

public static class Outer
{
    public class Inner
    {
        [AttributeUsage(AttributeTargets.All)]
        public class MyAttribute : Attribute
        {
        }
    }
}

[[|My|]]
class Test
{
}",
@"using System;
using static Outer.Inner;

public static class Outer
{
    public class Inner
    {
        [AttributeUsage(AttributeTargets.All)]
        public class MyAttribute : Attribute
        {
        }
    }
}

[My]
class Test
{
}");
        }

        [WorkItem(773614, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/773614")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddStaticType4()
        {
            var initialText =
@"using System;
using Outer;

public static class Outer
{
    public static class Inner
    {
        [AttributeUsage(AttributeTargets.All)]
        public class MyAttribute : Attribute
        {
        }
    }
}

[[|My|]]
class Test
{}";

            var expectedText =
@"using System;
using Outer;
using static Outer.Inner;

public static class Outer
{
    public static class Inner
    {
        [AttributeUsage(AttributeTargets.All)]
        public class MyAttribute : Attribute
        {
        }
    }
}

[My]
class Test
{}";

            await TestInRegularAndScriptAsync(initialText, expectedText);
        }

        [WorkItem(991463, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/991463")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddInsideUsingDirective1()
        {
            await TestInRegularAndScriptAsync(
@"namespace ns
{
    using B = [|Byte|];
}",
@"using System;

namespace ns
{
    using B = Byte;
}");
        }

        [WorkItem(991463, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/991463")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddInsideUsingDirective2()
        {
            await TestInRegularAndScriptAsync(
@"using System.Collections;

namespace ns
{
    using B = [|Byte|];
}",
@"using System;
using System.Collections;

namespace ns
{
    using B = Byte;
}");
        }

        [WorkItem(991463, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/991463")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddInsideUsingDirective3()
        {
            await TestInRegularAndScriptAsync(
@"namespace ns2
{
    namespace ns3
    {
        namespace ns
        {
            using B = [|Byte|];

            namespace ns4
            {
            }
        }
    }
}",
@"using System;

namespace ns2
{
    namespace ns3
    {
        namespace ns
        {
            using B = Byte;

            namespace ns4
            {
            }
        }
    }
}");
        }

        [WorkItem(991463, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/991463")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddInsideUsingDirective4()
        {
            await TestInRegularAndScriptAsync(
@"namespace ns2
{
    using System.Collections;

    namespace ns3
    {
        namespace ns
        {
            using System.IO;
            using B = [|Byte|];
        }
    }
}",
@"namespace ns2
{
    using System;
    using System.Collections;

    namespace ns3
    {
        namespace ns
        {
            using System.IO;
            using B = Byte;
        }
    }
}");
        }

        [WorkItem(991463, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/991463")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddInsideUsingDirective5()
        {
            await TestInRegularAndScriptAsync(
@"using System.IO;

namespace ns2
{
    using System.Diagnostics;

    namespace ns3
    {
        using System.Collections;

        namespace ns
        {
            using B = [|Byte|];
        }
    }
}",
@"using System.IO;

namespace ns2
{
    using System.Diagnostics;

    namespace ns3
    {
        using System;
        using System.Collections;

        namespace ns
        {
            using B = Byte;
        }
    }
}");
        }

        [WorkItem(991463, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/991463")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddInsideUsingDirective6()
        {
            await TestMissingInRegularAndScriptAsync(
@"using B = [|Byte|];");
        }

        [WorkItem(1064748, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1064748")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddConditionalAccessExpression()
        {
            var initialText =
@"<Workspace>
    <Project Language=""C#"" AssemblyName=""CSAssembly"" CommonReferences=""true"">
        <Document FilePath = ""Program"">
public class C
{
    void Main(C a)
    {
        C x = a?[|.B()|];
    }
}
       </Document>
       <Document FilePath = ""Extensions"">
namespace Extensions
{
    public static class E
    {
        public static C B(this C c) { return c; }
    }
}
        </Document>
    </Project>
</Workspace> ";

            var expectedText =
@"using Extensions;
public class C
{
    void Main(C a)
    {
        C x = a?.B();
    }
}";
            await TestInRegularAndScriptAsync(initialText, expectedText);
        }

        [WorkItem(1064748, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1064748")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddConditionalAccessExpression2()
        {
            var initialText =
@"<Workspace>
    <Project Language=""C#"" AssemblyName=""CSAssembly"" CommonReferences=""true"">
        <Document FilePath = ""Program"">
public class C
{
    public E B { get; private set; }

    void Main(C a)
    {
        int? x = a?.B.[|C()|];
    }

    public class E
    {
    }
}
       </Document>
       <Document FilePath = ""Extensions"">
namespace Extensions
{
    public static class D
    {
        public static C.E C(this C.E c) { return c; }
    }
}
        </Document>
    </Project>
</Workspace> ";

            var expectedText =
@"using Extensions;
public class C
{
    public E B { get; private set; }

    void Main(C a)
    {
        int? x = a?.B.C();
    }

    public class E
    {
    }
}";
            await TestInRegularAndScriptAsync(initialText, expectedText);
        }

        [WorkItem(1089138, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1089138")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAmbiguousUsingName()
        {
            await TestInRegularAndScriptAsync(
@"namespace ClassLibrary1
{
    using System;

    public class SomeTypeUser
    {
        [|SomeType|] field;
    }
}

namespace SubNamespaceName
{
    using System;

    class SomeType
    {
    }
}

namespace ClassLibrary1.SubNamespaceName
{
    using System;

    class SomeOtherFile
    {
    }
}",
@"namespace ClassLibrary1
{
    using System;
    using global::SubNamespaceName;

    public class SomeTypeUser
    {
        SomeType field;
    }
}

namespace SubNamespaceName
{
    using System;

    class SomeType
    {
    }
}

namespace ClassLibrary1.SubNamespaceName
{
    using System;

    class SomeOtherFile
    {
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingInDirective()
        {
            await TestInRegularAndScriptAsync(
@"#define DEBUG
#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
#endif
class Program
{
    static void Main(string[] args)
    {
        var a = [|File|].OpenRead("""");
    }
}",
@"#define DEBUG
#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.IO;
#endif
class Program
{
    static void Main(string[] args)
    {
        var a = File.OpenRead("""");
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingInDirective2()
        {
            await TestInRegularAndScriptAsync(
@"#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if DEBUG
using System.Text;
#endif
class Program { static void Main ( string [ ] args ) { var a = [|File|] . OpenRead ( """" ) ; } } ",
@"#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
#if DEBUG
using System.Text;
#endif
class Program { static void Main ( string [ ] args ) { var a = File . OpenRead ( """" ) ; } } ", ignoreTrivia: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingInDirective3()
        {
            await TestInRegularAndScriptAsync(
@"#define DEBUG
using System;
using System.Collections.Generic;
#if DEBUG
using System.Text;
#endif
using System.Linq;
using System.Threading.Tasks;
class Program { static void Main ( string [ ] args ) { var a = [|File|] . OpenRead ( """" ) ; } } ",
@"#define DEBUG
using System;
using System.Collections.Generic;
#if DEBUG
using System.Text;
#endif
using System.Linq;
using System.Threading.Tasks;
using System.IO;

class Program { static void Main ( string [ ] args ) { var a = File . OpenRead ( """" ) ; } } ", ignoreTrivia: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingInDirective4()
        {
            await TestInRegularAndScriptAsync(
@"#define DEBUG
#if DEBUG
using System;
#endif
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
class Program { static void Main ( string [ ] args ) { var a = [|File|] . OpenRead ( """" ) ; } } ",
@"#define DEBUG
#if DEBUG
using System;
#endif
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

class Program { static void Main ( string [ ] args ) { var a = File . OpenRead ( """" ) ; } } ", ignoreTrivia: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestInaccessibleExtensionMethod()
        {
            const string initial = @"
namespace N1
{
    public static class C
    {
        private static bool ExtMethod1(this string arg1)
        {
            return true;
        }
    }
}

namespace N2
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = ""str1"".[|ExtMethod1()|];
        }
    }
}";
            await TestMissingInRegularAndScriptAsync(initial);
        }

        [WorkItem(1116011, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1116011")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingForProperty()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    public BindingFlags BindingFlags
    {
        get
        {
            return BindingFlags.[|Instance|];
        }
    }
}",
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

class Program
{
    public BindingFlags BindingFlags
    {
        get
        {
            return BindingFlags.Instance;
        }
    }
}");
        }

        [WorkItem(1116011, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1116011")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingForField()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    public B B
    {
        get
        {
            return B.[|Instance|];
        }
    }
}

namespace A
{
    public class B
    {
        public static readonly B Instance;
    }
}",
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using A;

class Program
{
    public B B
    {
        get
        {
            return B.Instance;
        }
    }
}

namespace A
{
    public class B
    {
        public static readonly B Instance;
    }
}");
        }

        [WorkItem(1893, "https://github.com/dotnet/roslyn/issues/1893")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestNameSimplification()
        {
            // Generated using directive must be simplified from "using A.B;" to "using B;" below.
            await TestAsync(
@"namespace A.B
{
    class T1
    {
    }
}

namespace A.C
{
    using System;

    class T2
    {
        void Test()
        {
            Console.WriteLine();
            [|T1|] t1;
        }
    }
}",
@"namespace A.B
{
    class T1
    {
    }
}

namespace A.C
{
    using System;
    using A.B;

    class T2
    {
        void Test()
        {
            Console.WriteLine();
            T1 t1;
        }
    }
}", systemSpecialCase: true);
        }

        [WorkItem(935, "https://github.com/dotnet/roslyn/issues/935")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingWithOtherExtensionsInScope()
        {
            await TestInRegularAndScriptAsync(
@"using System.Linq;
using System.Collections;
using X;

namespace X
{
    public static class Ext
    {
        public static void ExtMethod(this int a)
        {
        }
    }
}

namespace Y
{
    public static class Ext
    {
        public static void ExtMethod(this int a, int v)
        {
        }
    }
}

public class B
{
    static void Main()
    {
        var b = 0;
        b.[|ExtMethod|](0);
    }
}",
@"using System.Linq;
using System.Collections;
using X;
using Y;

namespace X
{
    public static class Ext
    {
        public static void ExtMethod(this int a)
        {
        }
    }
}

namespace Y
{
    public static class Ext
    {
        public static void ExtMethod(this int a, int v)
        {
        }
    }
}

public class B
{
    static void Main()
    {
        var b = 0;
        b.ExtMethod(0);
    }
}");
        }

        [WorkItem(935, "https://github.com/dotnet/roslyn/issues/935")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingWithOtherExtensionsInScope2()
        {
            await TestInRegularAndScriptAsync(
@"using System.Linq;
using System.Collections;
using X;

namespace X
{
    public static class Ext
    {
        public static void ExtMethod(this int? a)
        {
        }
    }
}

namespace Y
{
    public static class Ext
    {
        public static void ExtMethod(this int? a, int v)
        {
        }
    }
}

public class B
{
    static void Main()
    {
        var b = new int?();
        b?[|.ExtMethod|](0);
    }
}",
@"using System.Linq;
using System.Collections;
using X;
using Y;

namespace X
{
    public static class Ext
    {
        public static void ExtMethod(this int? a)
        {
        }
    }
}

namespace Y
{
    public static class Ext
    {
        public static void ExtMethod(this int? a, int v)
        {
        }
    }
}

public class B
{
    static void Main()
    {
        var b = new int?();
        b?.ExtMethod(0);
    }
}");
        }

        [WorkItem(562, "https://github.com/dotnet/roslyn/issues/562")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingWithOtherExtensionsInScope3()
        {
            await TestInRegularAndScriptAsync(
@"using System.Linq;

class C
{
    int i = 0.[|All|]();
}

namespace X
{
    static class E
    {
        public static int All(this int o) => 0;
    }
}",
@"using System.Linq;
using X;

class C
{
    int i = 0.All();
}

namespace X
{
    static class E
    {
        public static int All(this int o) => 0;
    }
}");
        }

        [WorkItem(562, "https://github.com/dotnet/roslyn/issues/562")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingWithOtherExtensionsInScope4()
        {
            await TestInRegularAndScriptAsync(
@"using System.Linq;

class C
{
    static void Main(string[] args)
    {
        var a = new int?();
        int? i = a?[|.All|]();
    }
}

namespace X
{
    static class E
    {
        public static int? All(this int? o) => 0;
    }
}",
@"using System.Linq;
using X;

class C
{
    static void Main(string[] args)
    {
        var a = new int?();
        int? i = a?.All();
    }
}

namespace X
{
    static class E
    {
        public static int? All(this int? o) => 0;
    }
}");
        }

        [WorkItem(3080, "https://github.com/dotnet/roslyn/issues/3080")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestNestedNamespaceSimplified()
        {
            await TestInRegularAndScriptAsync(
@"namespace Microsoft.MyApp
{
    using Win32;

    class Program
    {
        static void Main(string[] args)
        {
            [|SafeRegistryHandle|] h;
        }
    }
}",
@"namespace Microsoft.MyApp
{
    using Microsoft.Win32.SafeHandles;
    using Win32;

    class Program
    {
        static void Main(string[] args)
        {
            SafeRegistryHandle h;
        }
    }
}");
        }

        [WorkItem(3080, "https://github.com/dotnet/roslyn/issues/3080")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestNestedNamespaceSimplified2()
        {
            await TestInRegularAndScriptAsync(
@"namespace Microsoft.MyApp
{
    using Zin32;

    class Program
    {
        static void Main(string[] args)
        {
            [|SafeRegistryHandle|] h;
        }
    }
}",
@"namespace Microsoft.MyApp
{
    using Microsoft.Win32.SafeHandles;
    using Zin32;

    class Program
    {
        static void Main(string[] args)
        {
            SafeRegistryHandle h;
        }
    }
}");
        }

        [WorkItem(3080, "https://github.com/dotnet/roslyn/issues/3080")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestNestedNamespaceSimplified3()
        {
            await TestInRegularAndScriptAsync(
@"namespace Microsoft.MyApp
{
    using System;
    using Win32;

    class Program
    {
        static void Main(string[] args)
        {
            [|SafeRegistryHandle|] h;
        }
    }
}",
@"namespace Microsoft.MyApp
{
    using System;
    using Microsoft.Win32.SafeHandles;
    using Win32;

    class Program
    {
        static void Main(string[] args)
        {
            SafeRegistryHandle h;
        }
    }
}");
        }

        [WorkItem(3080, "https://github.com/dotnet/roslyn/issues/3080")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestNestedNamespaceSimplified4()
        {
            await TestInRegularAndScriptAsync(
@"namespace Microsoft.MyApp
{
    using System;
    using Zin32;

    class Program
    {
        static void Main(string[] args)
        {
            [|SafeRegistryHandle|] h;
        }
    }
}",
@"namespace Microsoft.MyApp
{
    using System;
    using Microsoft.Win32.SafeHandles;
    using Zin32;

    class Program
    {
        static void Main(string[] args)
        {
            SafeRegistryHandle h;
        }
    }
}");
        }

        [WorkItem(3080, "https://github.com/dotnet/roslyn/issues/3080")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestNestedNamespaceSimplified5()
        {
            await TestInRegularAndScriptAsync(
@"namespace Microsoft.MyApp
{
#if true
    using Win32;
#else
    using System;
#endif
    class Program
    {
        static void Main(string[] args)
        {
            [|SafeRegistryHandle|] h;
        }
    }
}",
@"namespace Microsoft.MyApp
{
#if true
    using Microsoft.Win32.SafeHandles;
    using Win32;
#else
    using System;
#endif
    class Program
    {
        static void Main(string[] args)
        {
            SafeRegistryHandle h;
        }
    }
}");
        }

        [WorkItem(3080, "https://github.com/dotnet/roslyn/issues/3080")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestNestedNamespaceSimplified6()
        {
            await TestInRegularAndScriptAsync(
@"namespace Microsoft.MyApp
{
    using System;
#if false
    using Win32;
#endif
    using Win32;

    class Program
    {
        static void Main(string[] args)
        {
            [|SafeRegistryHandle|] h;
        }
    }
}",
@"namespace Microsoft.MyApp
{
    using System;
    using Microsoft.Win32.SafeHandles;
#if false
    using Win32;
#endif
    using Win32;

    class Program
    {
        static void Main(string[] args)
        {
            SafeRegistryHandle h;
        }
    }
}");
        }


        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingOrdinalUppercase()
        {
            await TestInRegularAndScriptAsync(
@"namespace A
{
    class A
    {
        static void Main(string[] args)
        {
            var b = new [|B|]();
        }
    }
}

namespace lowercase
{
    class b
    {
    }
}

namespace Uppercase
{
    class B
    {
    }
}",
@"using Uppercase;

namespace A
{
    class A
    {
        static void Main(string[] args)
        {
            var b = new B();
        }
    }
}

namespace lowercase
{
    class b
    {
    }
}

namespace Uppercase
{
    class B
    {
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingOrdinalLowercase()
        {
            await TestInRegularAndScriptAsync(
@"namespace A
{
    class A
    {
        static void Main(string[] args)
        {
            var a = new [|b|]();
        }
    }
}

namespace lowercase
{
    class b
    {
    }
}

namespace Uppercase
{
    class B
    {
    }
}",
@"using lowercase;

namespace A
{
    class A
    {
        static void Main(string[] args)
        {
            var a = new b();
        }
    }
}

namespace lowercase
{
    class b
    {
    }
}

namespace Uppercase
{
    class B
    {
    }
}");
        }

        [WorkItem(7443, "https://github.com/dotnet/roslyn/issues/7443")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestWithExistingIncompatibleExtension()
        {
            await TestInRegularAndScriptAsync(
@"using N;

class C
{
    int x()
    {
        System.Collections.Generic.IEnumerable<int> x = null;
        return x.[|Any|]
    }
}

namespace N
{
    static class Extensions
    {
        public static void Any(this string s)
        {
        }
    }
}",
@"using System.Linq;
using N;

class C
{
    int x()
    {
        System.Collections.Generic.IEnumerable<int> x = null;
        return x.Any
    }
}

namespace N
{
    static class Extensions
    {
        public static void Any(this string s)
        {
        }
    }
}");
        }

        [WorkItem(1744, @"https://github.com/dotnet/roslyn/issues/1744")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestIncompleteCatchBlockInLambda()
        {
            await TestInRegularAndScriptAsync(
@"class A
{
    System.Action a = () => {
    try
    {
    }
    catch ([|Exception|]",
@"using System;

class A
{
    System.Action a = () => {
    try
    {
    }
    catch (Exception");
        }

        [WorkItem(1033612, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1033612")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddInsideLambda()
        {
            var initialText =
@"using System;

static void Main(string[] args)
{
    Func<int> f = () => { [|List<int>|]. }
}";

            var expectedText =
@"using System;
using System.Collections.Generic;

static void Main(string[] args)
{
    Func<int> f = () => { List<int>.}
}";
            await TestInRegularAndScriptAsync(initialText, expectedText);
        }

        [WorkItem(1033612, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1033612")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddInsideLambda2()
        {
            var initialText =
@"using System;

static void Main(string[] args)
{
    Func<int> f = () => { [|List<int>|] }
}";

            var expectedText =
@"using System;
using System.Collections.Generic;

static void Main(string[] args)
{
    Func<int> f = () => { List<int>}
}";
            await TestInRegularAndScriptAsync(initialText, expectedText);
        }

        [WorkItem(1033612, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1033612")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddInsideLambda3()
        {
            var initialText =
@"using System;

static void Main(string[] args)
{
    Func<int> f = () => { 
        var a = 3;
        [|List<int>|].
        return a;
        };
}";

            var expectedText =
@"using System;
using System.Collections.Generic;

static void Main(string[] args)
{
    Func<int> f = () => { 
        var a = 3;
        List<int>.
        return a;
        };
}";
            await TestInRegularAndScriptAsync(initialText, expectedText);
        }

        [WorkItem(1033612, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1033612")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddInsideLambda4()
        {
            var initialText =
@"using System;

static void Main(string[] args)
{
    Func<int> f = () => { 
        var a = 3;
        [|List<int>|]
        return a;
        };
}";

            var expectedText =
@"using System;
using System.Collections.Generic;

static void Main(string[] args)
{
    Func<int> f = () => { 
        var a = 3;
        List<int>
        return a;
        };
}";
            await TestInRegularAndScriptAsync(initialText, expectedText);
        }

        [WorkItem(860648, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/860648")]
        [WorkItem(902014, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/902014")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestIncompleteParenthesizedLambdaExpression()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class Test
{
    void Foo()
    {
        Action a = () => {
            [|IBindCtx|] };
        string a;
    }
}",
@"using System;
using System.Runtime.InteropServices.ComTypes;

class Test
{
    void Foo()
    {
        Action a = () => {
            IBindCtx };
        string a;
    }
}");
        }

        [WorkItem(7461, "https://github.com/dotnet/roslyn/issues/7461")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestExtensionWithIncompatibleInstance()
        {
            await TestInRegularAndScriptAsync(
@"using System.IO;

namespace Namespace1
{
    static class StreamExtensions
    {
        public static void Write(this Stream stream, byte[] bytes)
        {
        }
    }
}

namespace Namespace2
{
    class Foo
    {
        void Bar()
        {
            Stream stream = null;
            stream.[|Write|](new byte[] { 1, 2, 3 });
        }
    }
}",
@"using System.IO;
using Namespace1;

namespace Namespace1
{
    static class StreamExtensions
    {
        public static void Write(this Stream stream, byte[] bytes)
        {
        }
    }
}

namespace Namespace2
{
    class Foo
    {
        void Bar()
        {
            Stream stream = null;
            stream.Write(new byte[] { 1, 2, 3 });
        }
    }
}");
        }

        [WorkItem(5499, "https://github.com/dotnet/roslyn/issues/5499")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestFormattingForNamespaceUsings()
        {
            await TestInRegularAndScriptAsync(
@"namespace N
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class Program
    {
        void Main()
        {
            [|Task<int>|]
        }
    }
}",
@"namespace N
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class Program
    {
        void Main()
        {
            Task<int>
        }
    }
}",
ignoreTrivia: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestGenericAmbiguityInSameNamespace()
        {
            await TestMissingInRegularAndScriptAsync(
@"namespace NS
{
    class C<T> where T : [|C|].N
    {
        public class N
        {
        }
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestNotOnVar1()
        {
            await TestMissingInRegularAndScriptAsync(
@"namespace N
{
    class var { }
}

class C
{
    void M()
    {
        [|var|]
    }
}
");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestNotOnVar2()
        {
            await TestMissingInRegularAndScriptAsync(
@"namespace N
{
    class Bar { }
}

class C
{
    void M()
    {
        [|var|]
    }
}
");
        }

        [WorkItem(226826, "https://devdiv.visualstudio.com/DevDiv/_workitems?id=226826")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingWithLeadingDocCommentInFrontOfUsing1()
        {
            await TestInRegularAndScriptAsync(
@"
/// Copyright 2016 - MyCompany 
/// All Rights Reserved 

using System;

class C : [|IEnumerable|]<int>
{
}
",
@"
/// Copyright 2016 - MyCompany 
/// All Rights Reserved 

using System;
using System.Collections.Generic;

class C : IEnumerable<int>
{
}
",
ignoreTrivia: false);
        }

        [WorkItem(226826, "https://devdiv.visualstudio.com/DevDiv/_workitems?id=226826")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingWithLeadingDocCommentInFrontOfUsing2()
        {
            await TestInRegularAndScriptAsync(
@"
/// Copyright 2016 - MyCompany 
/// All Rights Reserved 

using System.Collections;

class C
{
    [|DateTime|] d;
}
",
@"
/// Copyright 2016 - MyCompany 
/// All Rights Reserved 

using System;
using System.Collections;

class C
{
    DateTime d;
}
",
ignoreTrivia: false);
        }

        [WorkItem(226826, "https://devdiv.visualstudio.com/DevDiv/_workitems?id=226826")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestAddUsingWithLeadingDocCommentInFrontOfClass1()
        {
            await TestInRegularAndScriptAsync(
@"
/// Copyright 2016 - MyCompany 
/// All Rights Reserved 
class C
{
    [|DateTime|] d;
}
",
@"
using System;
/// Copyright 2016 - MyCompany 
/// All Rights Reserved 
class C
{
    DateTime d;
}
",
ignoreTrivia: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestPlaceUsingWithUsings_NotWithAliases()
        {
            await TestInRegularAndScriptAsync(
@"
using System;

namespace N
{
    using C = System.Collections;

    class Class
    {
        [|List<int>|] Method()
        {
            Foo();
        }
    }
}",
@"
using System;
using System.Collections.Generic;

namespace N
{
    using C = System.Collections;

    class Class
    {
        List<int> Method()
        {
            Foo();
        }
    }
}");
        }

        [WorkItem(15025, "https://github.com/dotnet/roslyn/issues/15025")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestPreferSystemNamespaceFirst()
        {
            await TestInRegularAndScriptAsync(
@"
namespace Microsoft
{
    public class SomeClass { }
}

namespace System
{
    public class SomeClass { }
}

namespace N
{
    class Class
    {
        [|SomeClass|] c;
    }
}",
@"
using System;

namespace Microsoft
{
    public class SomeClass { }
}

namespace System
{
    public class SomeClass { }
}

namespace N
{
    class Class
    {
        SomeClass c;
    }
}");
        }

        [WorkItem(15025, "https://github.com/dotnet/roslyn/issues/15025")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestPreferSystemNamespaceFirst2()
        {
            await TestInRegularAndScriptAsync(
@"
namespace Microsoft
{
    public class SomeClass { }
}

namespace System
{
    public class SomeClass { }
}

namespace N
{
    class Class
    {
        [|SomeClass|] c;
    }
}",
@"
using Microsoft;

namespace Microsoft
{
    public class SomeClass { }
}

namespace System
{
    public class SomeClass { }
}

namespace N
{
    class Class
    {
        SomeClass c;
    }
}", index: 1);
        }

        [WorkItem(18275, "https://github.com/dotnet/roslyn/issues/18275")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddImport)]
        public async Task TestContextualKeyword1()
        {
            await TestMissingInRegularAndScriptAsync(
@"
namespace N
{
    class nameof
    {
    }
}

class C
{
    void M()
    {
        [|nameof|]
    }
}");
        }
    }
}