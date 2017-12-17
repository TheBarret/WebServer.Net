Imports Xm.Elements
Imports Xm.Elements.Types
Imports System.IO
Imports System.Reflection

Public NotInheritable Class Runtime
    Inherits Stack(Of Scope)
    Implements IDisposable
    Public Property Input As TextReader
    Public Property Output As TextWriter
    Public Property Context As List(Of Expression)
    Public Property Functions As List(Of [Delegate])
    Public Property Globals As Dictionary(Of String, Object)
    Private Property Loaded As Boolean
    ''' <summary>
    ''' Constructors
    ''' </summary>
    Sub New(Context As String)
        Me.Loaded = True
        Me.Functions = New List(Of [Delegate])
        Me.Globals = New Dictionary(Of String, Object)
        Me.Context = New AST.Parser().Analyze(New Lexer.Parser().Analyze(Context))
    End Sub
    Sub New(Tokens As List(Of Token))
        Me.Loaded = True
        Me.Functions = New List(Of [Delegate])
        Me.Globals = New Dictionary(Of String, Object)
        Me.Context = New AST.Parser().Analyze(Tokens)
    End Sub
    ''' <summary>
    ''' Register a delegate
    ''' </summary>
    Public Sub AddFunction(d As [Delegate])
        Me.Functions.Add(d)
    End Sub
    ''' <summary>
    ''' Register a global variable
    ''' </summary>
    Public Sub AddGlobal(name As String, value As Object)
        Me.Globals.Add(String.Format("${0}", name.ToLower), value)
    End Sub
    Public Sub AddGlobal(Of X, Y)(collection As Dictionary(Of X, Y))
        For Each pair As KeyValuePair(Of X, Y) In collection
            Me.Globals.Add(String.Format("$_{0}", pair.Key.ToString.ToLower), pair.Value)
        Next
    End Sub
    ''' <summary>
    ''' Output stream
    ''' </summary>
    Public Sub SetOutput(output As TextWriter)
        Me.Output = output
    End Sub
    ''' <summary>
    ''' Input stream
    ''' </summary>
    Public Sub SetInput(input As TextReader)
        Me.Input = input
    End Sub
    ''' <summary>
    ''' Direct function call
    ''' </summary>
    Public Function [Call](Of T)(Name As String, ParamArray Parameters() As Object) As T
        Try
            Me.EnterScope()
            Me.StoreGlobals()
            Me.StoreFunctions()
            Me.Scope.StoreScriptFunctions(Context)
            If (Me.Scope.Variable(Name)) Then
                Dim func As TValue = Me.Scope.GetVariable(Name)
                If (func.IsFunction) Then
                    Return CType(Helpers.Unwrap(Me.Resolve(func.Cast(Of TFunction), Parameters.ToList)), T)
                End If
            End If
            Throw New Exception(String.Format("Undefined function '{0}'", Name))
        Finally
            Me.LeaveScope()
        End Try
    End Function
    Public Function [Call](Name As String, ParamArray Parameters() As Object) As Object
        Try
            Me.EnterScope()
            Me.StoreGlobals()
            Me.StoreFunctions()
            Me.Scope.StoreScriptFunctions(Context)
            If (Me.Scope.Variable(Name)) Then
                Dim func As TValue = Me.Scope.GetVariable(Name)
                If (func.IsFunction) Then
                    Return Helpers.Unwrap(Me.Resolve(func.Cast(Of TFunction), Parameters.ToList))
                End If
            End If
            Throw New Exception(String.Format("Undefined function '{0}'", Name))
        Finally
            Me.LeaveScope()
        End Try
    End Function
    ''' <summary>
    ''' Evaluate with new scope
    ''' </summary>
    Public Function Evaluate(Of T)(ParamArray Parameters() As Object) As T
        If (Me.Loaded AndAlso Me.Context IsNot Nothing AndAlso Me.Context.Any) Then
            Return Me.EvaluateContext(Of T)(Me.Context, Parameters)
        End If
        Return Nothing
    End Function
    Public Function Evaluate(ParamArray Parameters() As Object) As Object
        If (Me.Loaded AndAlso Me.Context IsNot Nothing AndAlso Me.Context.Any) Then
            Return Me.EvaluateContext(Me.Context, Parameters)
        End If
        Return Nothing
    End Function
    Private Function EvaluateContext(Of T)(Context As List(Of Expression), ParamArray Parameters() As Object) As T
        Return CType(Me.EvaluateContext(Context, Parameters), T)
    End Function
    Private Function EvaluateContext(Context As List(Of Expression), ParamArray Parameters() As Object) As Object
        Try
            Me.EnterScope()
            Me.StoreFunctions()
            Me.StoreGlobals()
            Me.Scope.StoreScriptFunctions(Context)
            Me.Scope.StoreParameters(Parameters)
            Dim lastValue As TValue = Nothing
            For Each e As Expression In Context
                lastValue = Me.Resolve(e)
                If (Me.Scope.IsReturn) Then Exit For
            Next
            ''' Single line or script defined return
            If (Context.Count = 1 AndAlso Not Me.Scope.IsReturn) Then
                Return Helpers.Unwrap(lastValue)
            Else
                Return Helpers.Unwrap(Me.Scope.ReturnValue)
            End If
        Finally
            Me.Scope.Dispose()
            Me.LeaveScope()
        End Try
    End Function
    ''' <summary>
    ''' Collect defined delegates
    ''' </summary>
    Private Sub StoreFunctions()
        Me.Functions.ForEach(Sub(d) Me.Scope.SetVariable(d.Method.Name, New TValue(d)))
        Me.Functions.Clear()
    End Sub
    ''' <summary>
    ''' Collect defined globals
    ''' </summary>
    Private Sub StoreGlobals()
        Me.Globals.ForEach(Sub(x, y) Me.Scope.SetVariable(x, New TValue(y)))
        Me.Globals.Clear()
    End Sub
    ''' <summary>
    ''' Evaluate with no new scope
    ''' </summary>
    Private Function EvaluateContextNoScope(Context As List(Of Expression)) As Object
        Dim lastValue As TValue = Nothing
        For Each e As Expression In Context
            lastValue = Me.Resolve(e)
            If (Me.Scope.IsReturn) Then Exit For
        Next
        ''' Single line or script defined return
        If (Context.Count = 1 AndAlso Not Me.Scope.IsReturn) Then
            Return Helpers.Unwrap(lastValue)
        Else
            Return Helpers.Unwrap(Me.Scope.ReturnValue)
        End If
    End Function
    ''' <summary>
    ''' Resolve expression
    ''' </summary>
    Private Function Resolve(e As Expression) As TValue
        If (TypeOf e Is TUse) Then
            Return Me.Resolve(CType(e, TUse))
        ElseIf (TypeOf e Is TNull) Then
            Return New TValue
        ElseIf (TypeOf e Is TString) Then
            Return New TValue(CType(e, TString).Escaped)
        ElseIf (TypeOf e Is TBoolean) Then
            Return New TValue(CType(e, TBoolean).Value)
        ElseIf (TypeOf e Is TInteger) Then
            Return New TValue(CType(e, TInteger).Value)
        ElseIf (TypeOf e Is TFloat) Then
            Return New TValue(CType(e, TFloat).Value)
        ElseIf (TypeOf e Is TIdentifier) Then
            Return Me.Resolve(CType(e, TIdentifier))
        ElseIf (TypeOf e Is TCall) Then
            Return Me.Resolve(CType(e, TCall))
        ElseIf (TypeOf e Is TMember) Then
            Return Me.Resolve(CType(e, TMember))
        ElseIf (TypeOf e Is TReturn) Then
            Return Me.Resolve(CType(e, TReturn))
        ElseIf (TypeOf e Is Binary) Then
            Return Me.Resolve(CType(e, Binary))
        ElseIf (TypeOf e Is Unary) Then
            Return Me.Resolve(CType(e, Unary))
        ElseIf (TypeOf e Is TConditional) Then
            Return Me.Resolve(CType(e, TConditional))
        ElseIf (TypeOf e Is TFunction) Then
            Return New TValue(CType(e, TFunction))
        End If
        Throw New Exception(String.Format("Undefined expression type '{0}'", e.GetType.Name))
    End Function
    ''' <summary>
    ''' Import external libraries
    ''' </summary>
    Private Function Resolve(e As TUse) As TValue
        Dim reference As String = Me.Scope.GetStringValue(e.Library)
        If (reference.Contains(".dll")) Then
            If (File.Exists(String.Format(".\{0}", reference))) Then
                For Each t As Type In Assembly.LoadFile(Path.GetFullPath(String.Format(".\{0}", reference))).GetTypes
                    Me.Scope.Scan(t)
                Next
            End If
        Else
            Dim typeRef As Type = Type.GetType(reference, False, True)
            If (typeRef IsNot Nothing) Then Me.Scope.Scan(typeRef)
        End If
        Return TValue.Null
    End Function
    ''' <summary>
    ''' Resolve binary expression
    ''' </summary>
    Private Function Resolve(e As Binary) As TValue
        If (e.Op = Tokens.T_Assign) Then
            Dim name As String = Me.Scope.GetStringValue(e.Left)
            If (Not name.StartsWith("$")) Then
                Dim operand As TValue = Me.Resolve(e.Right)
                Me.Scope.SetVariable(CType(e.Left, TIdentifier).Value, operand)
                Return operand
            Else
                Throw New Exception("Attempted to write readonly variable.")
            End If
        Else
            Dim left As TValue = Me.Resolve(e.Left)
            Dim right As TValue = Me.Resolve(e.Right)
            If (e.Op = Tokens.T_Plus) Then
                Return Helpers.Addition(left, right)
            ElseIf (e.Op = Tokens.T_Minus) Then
                Return Helpers.Subtraction(left, right)
            ElseIf (e.Op = Tokens.T_Mult) Then
                Return Helpers.Multiplication(left, right)
            ElseIf (e.Op = Tokens.T_Div) Then
                Return Helpers.Division(left, right)
            ElseIf (e.Op = Tokens.T_Mod) Then
                Return Helpers.Modulo(left, right)
            ElseIf (e.Op = Tokens.T_And) Then
                Return Helpers.LogicalAnd(left, right)
            ElseIf (e.Op = Tokens.T_Or) Then
                Return Helpers.LogicalOr(left, right)
            ElseIf (e.Op = Tokens.T_Xor) Then
                Return Helpers.LogicalXor(left, right)
            ElseIf (e.Op = Tokens.T_Equal) Then
                Return Helpers.IsEqual(left, right)
            ElseIf (e.Op = Tokens.T_NotEqual) Then
                Return Helpers.IsNotEqual(left, right)
            ElseIf (e.Op = Tokens.T_Greater) Then
                Return Helpers.IsGreater(left, right)
            ElseIf (e.Op = Tokens.T_Lesser) Then
                Return Helpers.IsLesser(left, right)
            ElseIf (e.Op = Tokens.T_EqualOrGreater) Then
                Return Helpers.IsEqualOrGreater(left, right)
            ElseIf (e.Op = Tokens.T_EqualOrLesser) Then
                Return Helpers.IsEqualOrLesser(left, right)
            End If
        End If
        Throw New Exception(String.Format("Undefined expression type '{0}'", e.GetType.Name))
    End Function
    ''' <summary>
    ''' Resolve unary expression
    ''' </summary>
    Private Function Resolve(e As Unary) As TValue
        If (e.Op = Tokens.T_Negate) Then
            Return Helpers.Negate(Me.Resolve(e.Operand))
        ElseIf (e.Op = Tokens.T_Plus) Then
            Return Helpers.SignedPositive(Me.Resolve(e.Operand))
        ElseIf (e.Op = Tokens.T_Minus) Then
            Return Helpers.SignedNegative(Me.Resolve(e.Operand))
        End If
        Throw New Exception(String.Format("Undefined expression type '{0}'", e.GetType.Name))
    End Function
    ''' <summary>
    ''' Resolve if condition block
    ''' </summary>
    Private Function Resolve(e As TConditional) As TValue
        Dim condition As TValue = Me.Resolve(e.Condition)
        If (condition.IsBoolean) Then
            If (condition.Cast(Of Boolean)()) Then
                Me.EvaluateContextNoScope(e.True)
            Else
                Me.EvaluateContextNoScope(e.False)
            End If
            Return condition
        Else
            Throw New Exception(String.Format("Expecting boolean in '{0}' statement", e.ToString))
        End If
    End Function
    ''' <summary>
    ''' Resolve identifier
    ''' </summary>
    Private Function Resolve(e As TIdentifier) As TValue
        Dim name As String = e.Value
        If (Me.Scope.Variable(name)) Then Return Me.Scope.GetVariable(name)
        Throw New Exception(String.Format("Undefined identifier '{0}'", name))
    End Function
    ''' <summary>
    ''' Resolve member
    ''' </summary>
    Private Function Resolve(e As TMember) As TValue
        Dim value As TValue = Me.Resolve(e.Target)
        Dim reference As String = String.Format("{0}.{1}", value.ScriptType, Me.Scope.GetStringValue(e.Member))
        If (Me.Scope.Variable(reference)) Then
            Dim func As TValue = Me.Scope.GetVariable(reference)
            If (func.IsDelegate) Then
                Return Me.Resolve(CType(func.Value, [Delegate]), value.AsList(Me.Resolve(e.Parameters)))
            End If
        End If
        Throw New Exception(String.Format("Undefined function '{0}'", e.ToString))
    End Function
    ''' <summary>
    ''' Resolve call
    ''' </summary>
    Private Function Resolve(e As TCall) As TValue
        If (Me.Scope.Variable(Me.Scope.GetStringValue(e.Name))) Then
            Dim func As TValue = Me.Resolve(e.Name)
            If (func.IsFunction) Then
                Return Me.Resolve(func.Cast(Of TFunction), e.Parameters)
            ElseIf (func.IsDelegate) Then
                Return Me.Resolve(CType(func.Value, [Delegate]), Me.Resolve(e.Parameters))
            End If
            Throw New Exception(String.Format("Unexpected '{0}', expecting function or delegate", func.ScriptType))
        ElseIf (TypeOf e.Name Is TFunction) Then
            Return Me.Resolve(CType(e.Name, TFunction), e.Parameters)
        End If
        Throw New Exception(String.Format("Undefined function '{0}'", e.ToString))
    End Function
    ''' <summary>
    ''' Resolve TFunction
    ''' </summary>
    Private Function Resolve(e As TFunction, params As List(Of Object)) As Object
        Try
            Me.EnterScope()
            If (e.Parameters.Count = params.Count) Then
                For i As Integer = 0 To e.Parameters.Count - 1
                    Me.Scope.SetVariable(Me.Scope.GetStringValue(e.Parameters(i)), New TValue(params(i)))
                Next
                Return New TValue(Me.EvaluateContextNoScope(e.Body))
            End If
        Finally
            Me.LeaveScope()
        End Try
        Throw New Exception(String.Format("Parameter count mismatch for '{0}'", e.ToString))
    End Function
    Private Function Resolve(e As TFunction, params As List(Of Expression)) As TValue
        Try
            Me.EnterScope()
            If (e.Parameters.Count = params.Count) Then
                For i As Integer = 0 To e.Parameters.Count - 1
                    Me.Scope.SetVariable(Me.Scope.GetStringValue(e.Parameters(i)), Me.Resolve(params(i)))
                Next
                Return New TValue(Me.EvaluateContextNoScope(e.Body))
            End If
        Finally
            Me.LeaveScope()
        End Try
        Throw New Exception(String.Format("Parameter count mismatch for '{0}'", e.ToString))
    End Function
    ''' <summary>
    ''' Resolve method
    ''' </summary>
    Private Function Resolve(e As [Delegate], params As List(Of TValue)) As TValue
        Try
            Me.EnterScope()
            If (e.RequiresRuntime) Then params.Insert(0, New TValue(Me))
            If (Me.Validate(e, params)) Then
                Return New TValue(e.Method.Invoke(Nothing, params.Select(Function(p) p.Value).ToArray))
            End If
            Return TValue.Null
        Finally
            Me.LeaveScope()
        End Try
    End Function
    ''' <summary>
    ''' Validates the delegate signature with given parameters
    ''' </summary>
    Private Function Validate(e As [Delegate], params As List(Of TValue)) As Boolean
        If (e.Method.GetParameters.Count <> params.Count) Then
            Throw New Exception(String.Format("Parameter count mismatch for '{0}()'", e.Method.Name))
        End If
        For i As Integer = 0 To e.Method.GetParameters.Count - 1
            If (e.Method.GetParameters(i).ParameterType Is GetType(Object)) Then Continue For
            If (e.Method.GetParameters(i).ParameterType IsNot params(i).Value.GetType) Then
                Throw New Exception(String.Format("Parameter type mismatch for '{0}()'", e.Method.Name))
            End If
        Next
        Return True
    End Function
    ''' <summary>
    ''' Resolve collection
    ''' </summary>
    Private Function Resolve(e As List(Of Expression)) As List(Of TValue)
        Return e.Select(Function(exp) Me.Resolve(exp)).ToList
    End Function
    ''' <summary>
    ''' Resolve return expression
    ''' </summary>
    Private Function Resolve(e As TReturn) As TValue
        Me.Scope.IsReturn = True
        Me.Scope.ReturnValue = Me.Resolve(e.Operand)
        Return TValue.Null
    End Function
    ''' <summary>
    ''' Scope control: current
    ''' </summary>
    Public Function Scope() As Scope
        Return Me.Peek
    End Function
    ''' <summary>
    ''' Scope control: enter
    ''' </summary>
    Protected Friend Sub EnterScope()
        Me.Push(New Scope(Me))
        If (Me.Count = 1) Then
            Me.Scope.Scan(GetType(Library.Common))
            Me.Scope.Scan(GetType(Library.IO))
            Me.Scope.Scan(GetType(Library.Any))
            Me.Scope.Scan(GetType(Library.String))
            Me.Scope.Scan(GetType(Library.Integer))
            Me.Scope.Scan(GetType(Library.Float))
        End If
    End Sub
    ''' <summary>
    ''' Scope control: leave
    ''' </summary>
    Protected Friend Sub LeaveScope()
        If (Me.Count > 0) Then
            Me.Scope.Dispose()
            Me.Pop()
        End If
    End Sub
    ''' <summary>
    ''' IDisposable Support
    ''' </summary>
    Private disposedValue As Boolean
    Protected Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                For Each sc In Me
                    sc.Dispose()
                Next
                Me.Clear()
                Me.Functions.Clear()
                Me.Globals.Clear()
                Me.Context.Clear()
            End If
        End If
        Me.disposedValue = True
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        Me.Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
End Class
