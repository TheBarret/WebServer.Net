Imports System.IO
Imports System.Xml
Imports System.Text

Public Class Generator
    Inherits List(Of Byte)
    Public Const README_FILENAME As String = "readme.md"
    Public Const README_TAGS As String = "<!-- README.MD -->"
    Public Property Client As Client
    Private Property Elements As Dictionary(Of Integer, String)
    Sub New(Client As Client)
        Me.Client = Client
        Me.Elements = New Dictionary(Of Integer, String)
    End Sub
    Public Function DirectoryList(Dir As String) As Byte()
        If (File.Exists(Me.Client.GetLocalPath(Me.Client.Environment.DirectoryTemplate)) AndAlso Directory.Exists(Dir)) Then
            Dim document As New XmlDocument, base As XmlElement, address As String = Me.Client.GetRelativePath(Dir)
            Using fs As New FileStream(Me.Client.GetLocalPath(Me.Client.Environment.DirectoryTemplate), FileMode.Open, FileAccess.Read, FileShare.None)
                document.Load(fs)
            End Using
            base = document.GetElementsByTagName("DirectoryTemplate").Cast(Of XmlElement)().First
            If (Generator.HasDefinedBody(base) And Generator.HasDefinedFolder(base) And Generator.HasDefinedFile(base)) Then
                Dim output As New List(Of String)
                Me.Elements.Add(0, base.SelectSingleNode("Body").InnerText.Replace("{LOCATION}", address))
                Me.Elements.Add(1, base.SelectSingleNode("Folders").InnerText)
                Me.Elements.Add(2, base.SelectSingleNode("Files").InnerText)
                For Each d As String In Directory.GetDirectories(Dir)
                    Dim current As New DirectoryInfo(d)
                    If (Me.Client.Environment.HideDotNames And current.Name.StartsWith(".")) Then
                        Continue For
                    End If
                    output.Add(String.Format(Me.Elements(1), address, current.Name, current.LastWriteTime.ToString("r")))
                Next
                For Each f As String In Directory.GetFiles(Dir)
                    Dim current As New FileInfo(f)
                    If (Not Me.Client.IsHiddenFileType(f)) Then
                        If (Me.Client.Environment.HideDotNames And current.Name.StartsWith(".")) Then
                            Continue For
                        End If
                        If (current.Name.ToLower.Equals(Generator.README_FILENAME)) Then
                            Using sr As New StreamReader(New FileStream(f, FileMode.Open, FileAccess.Read, FileShare.None))
                                Me.Elements(0) = Me.Elements(0).Replace(Generator.README_TAGS, sr.ReadToEnd)
                            End Using
                        End If
                        output.Add(String.Format(Me.Elements(2), address, current.Name, current.LastWriteTime.ToString("r"), current.Length.HumanReadable))
                    End If
                Next
                Me.AddRange(Me.Client.Environment.Encoder.GetBytes(Me.Elements(0).Replace("{TABLE}", String.Join(Environment.NewLine, output))))
            End If
        End If
        Return Me.ToArray
    End Function
    ''' <summary>
    ''' Validates XML document
    ''' </summary>
    Public Shared Function HasDefinedBody(node As XmlElement) As Boolean
        Return node.SelectSingleNode("Body") IsNot Nothing
    End Function
    ''' <summary>
    ''' Validates XML document
    ''' </summary>
    Public Shared Function HasDefinedFolder(node As XmlElement) As Boolean
        Return node.SelectSingleNode("Folders") IsNot Nothing
    End Function
    ''' <summary>
    ''' Validates XML document
    ''' </summary>
    Public Shared Function HasDefinedFile(node As XmlElement) As Boolean
        Return node.SelectSingleNode("Files") IsNot Nothing
    End Function
End Class
