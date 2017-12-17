Imports Xm.Elements
Imports Xm.Elements.Types
Imports System.IO
Imports System.Reflection

<Serializable>
Public NotInheritable Class Scope
    Inherits Dictionary(Of String, TValue)
    Implements IDisposable
    Public Property Parent As Runtime
    Public Property IsReturn As Boolean
    Public Property ReturnValue As Object
    Sub New(Parent As Runtime)
        Me.Parent = Parent
        Me.IsReturn = False
    End Sub
    Public Sub SetVariable(Name As String, value As TValue)
        If (Me.ContainsKey(Name.ToLower)) Then
            Me(Name.ToLower) = value
        Else
            Me.Add(Name.ToLower, value)
        End If
    End Sub
    Public Function GetVariable(Name As String) As TValue
        For Each Scope In Me.Parent
            For Each var In Scope
                If (var.Key.Equals(Name.ToLower) OrElse Name.ToLower Like var.Key) Then
                    Return var.Value
                End If
            Next
        Next
        Return TValue.Null
    End Function
    Public Function Variable(Name As String) As Boolean
        For Each Scope In Me.Parent
            For Each var In Scope
                If (var.Key.Equals(Name.ToLower) OrElse Name.ToLower Like var.Key) Then
                    Return True
                End If
            Next
        Next
        Return False
    End Function
    ''' <summary>
    ''' Collects library functions
    ''' </summary>
    Public Sub Scan(type As Type)
        For Each m As MethodInfo In type.GetMethods(BindingFlags.Public Or BindingFlags.Static)
            For Each attr As Method In m.GetCustomAttributes.OfType(Of Method)()
                If (attr.Type = Types.Null) Then
                    Me.SetVariable(attr.Reference, New TValue(Helpers.CreateDelegate(type, m.Name)))
                ElseIf (attr.Type = Types.Any) Then
                    Me.SetVariable(String.Format("*.{1}", attr.Type, attr.Reference), New TValue(Helpers.CreateDelegate(type, m.Name)))
                Else
                    Me.SetVariable(String.Format("{0}.{1}", attr.Type, attr.Reference), New TValue(Helpers.CreateDelegate(type, m.Name)))
                End If
            Next
        Next
    End Sub
    ''' <summary>
    ''' Collect functions inside context
    ''' </summary>
    Public Sub StoreScriptFunctions(Context As List(Of Expression))
        For Each e As Expression In Context.ToList
            If (TypeOf e Is TFunction) Then
                Me.SetVariable(Me.GetStringValue(CType(e, TFunction)), New TValue(e))
                Context.Remove(e)
            End If
        Next
    End Sub
    ''' <summary>
    ''' Store parameters
    ''' </summary>
    Public Sub StoreParameters(ParamArray Parameters() As Object)
        For i As Integer = 0 To Parameters.Length - 1
            Me.SetVariable(String.Format("arg{0}", i + 1), New TValue(Parameters(i)))
        Next
    End Sub
    ''' <summary>
    ''' Get string value from expression type
    ''' </summary>
    Public Function GetStringValue(e As Expression) As String
        Select Case e.GetType
            Case GetType(TString)
                Return CType(e, TString).Value
            Case GetType(TIdentifier)
                Return CType(e, TIdentifier).Value
            Case GetType(TInteger)
                Return CType(e, TInteger).Value.ToString
            Case GetType(TFloat)
                Return CType(e, TFloat).Value.ToString
            Case GetType(TBoolean)
                Return CType(e, TBoolean).Value.ToString
            Case GetType(TFunction)
                Return Me.GetStringValue(CType(e, TFunction).Name)
            Case Else
                Return e.ToString
        End Select
    End Function
#Region "IDisposable Support"
    Private disposedValue As Boolean
    Protected Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                Me.Clear()
            End If
        End If
        Me.disposedValue = True
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        Me.Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
