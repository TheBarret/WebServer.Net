Imports System.IO
Imports System.Xml

Public Class Access
    ''' <summary>
    ''' Returns boolean if client can access this location if access config exists
    ''' </summary>
    Public Shared Function Match(Client As Client, Base As String, Filename As String) As Boolean
        Return Access.Match(Client, String.Format("{0}{1}", Base, Filename))
    End Function
    Public Shared Function Match(Client As Client, Filename As String) As Boolean
        Dim rules As New Dictionary(Of String, TypeAccess)
        If (File.Exists(Filename)) Then
            Dim document As New XmlDocument, base As XmlElement
            Using fs As New FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.None)
                document.Load(fs)
            End Using
            If (Access.HasDefinedRules(document)) Then
                base = Access.GetElement(document, "Rules")
                For Each rule As XmlElement In base.GetElementsByTagName("Rule")
                    rules.Add(rule.InnerText.Trim, Access.GetTypeAccess(rule.GetAttribute("Action")))
                Next
            End If
        End If
        For Each rule As KeyValuePair(Of String, TypeAccess) In rules
            If (Client.RemoteAddress Like rule.Key) Then
                Return rule.Value = TypeAccess.Allow
            End If
        Next
        Return False
    End Function
    ''' <summary>
    ''' Returns the enum type of given string based rule access
    ''' </summary>
    Public Shared Function GetTypeAccess(AccessType As String) As TypeAccess
        If (AccessType.ToLower.Equals("allow")) Then
            Return TypeAccess.Allow
        ElseIf (AccessType.ToLower.Equals("deny")) Then
            Return TypeAccess.Deny
        End If
        Return TypeAccess.Undefined
    End Function
    ''' <summary>
    ''' Cast the first XML element by reference
    ''' </summary>
    Public Shared Function GetElement(node As XmlDocument, Reference As String) As XmlElement
        Return node.GetElementsByTagName(Reference).Cast(Of XmlElement)().First
    End Function
    ''' <summary>
    ''' Validates XML element
    ''' </summary>
    Public Shared Function HasDefinedRules(node As XmlDocument) As Boolean
        Return node.SelectSingleNode("Rules") IsNot Nothing
    End Function
End Class
