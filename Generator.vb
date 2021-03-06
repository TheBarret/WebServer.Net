﻿Imports System.IO
Imports System.Xml
Imports System.Text
Imports System.Net

Public Class Generator
    Inherits List(Of Byte)
    Public Property Client As Client
    Private Property Elements As Dictionary(Of ElementType, String)
    Sub New(Client As Client)
        Me.Client = Client
        Me.Elements = New Dictionary(Of ElementType, String)
    End Sub
    Public Function ErrorPage(StatusCode As HttpStatusCode) As Byte()
        If (File.Exists(Me.Client.LocalPath(Me.Client.Settings.ErrorPageTemplate))) Then
            Dim document As New XmlDocument, base As XmlElement
            Using fs As New FileStream(Me.Client.LocalPath(Me.Client.Settings.ErrorPageTemplate), FileMode.Open, FileAccess.Read, FileShare.None)
                document.Load(fs)
            End Using
            If (Generator.HasDefinedTemplate(document)) Then
                base = Generator.GetElement(document, "Templates")
                If (Generator.HasDefinedErrorTemplate(base)) Then
                    base = Generator.GetElement(document, "ErrorTemplate")
                    '// Generate body
                    If (Generator.HasDefinedBody(base)) Then
                        Me.Elements.Add(ElementType.Body, Generator.Trim(base.SelectSingleNode("Body").InnerText))
                        Me.Elements(ElementType.Body) = Me.Elements(ElementType.Body).Replace("{0}", CType(StatusCode, Int32).ToString)
                        Me.Elements(ElementType.Body) = Me.Elements(ElementType.Body).Replace("{1}", StatusCode.ToString)
                        '// Append to byte array
                        Me.AddRange(Me.Client.Encoding.GetBytes(Me.Elements(ElementType.Body)))
                    End If
                End If
            End If
        End If
        Return Me.ToArray
    End Function
    Public Function DirectoryPage(Dir As String) As Byte()
        If (File.Exists(Me.Client.LocalPath(Me.Client.Settings.DirectoryTemplate)) AndAlso Directory.Exists(Dir)) Then
            Dim document As New XmlDocument, base As XmlElement, relativeAddress As String = Me.Client.RelativePath(Dir)
            Using fs As New FileStream(Me.Client.LocalPath(Me.Client.Settings.DirectoryTemplate), FileMode.Open, FileAccess.Read, FileShare.None)
                document.Load(fs)
            End Using
            If (Generator.HasDefinedTemplate(document)) Then
                base = Generator.GetElement(document, "Templates")
                If (Generator.HasDefinedDirTemplate(base)) Then
                    base = Generator.GetElement(base, "DirectoryTemplate")
                    '// Readme.md Support
                    If (Generator.HasDefinedReadMe(base)) Then
                        Dim readme As XmlElement = Generator.GetElement(base, "Readme")
                        Me.Elements.Add(ElementType.Readme, readme.GetAttribute("Filename"))
                        Me.Elements.Add(ElementType.ReadmeTag, Generator.Trim(readme.InnerText))
                    End If
                    '// Generate body
                    If (Generator.HasDefinedBody(base) And Generator.HasDefinedFolder(base) And Generator.HasDefinedFile(base)) Then
                        Dim output As New List(Of String)
                        Me.Elements.Add(ElementType.Body, Generator.Trim(base.SelectSingleNode("Body").InnerText).Replace("{LOCATION}", relativeAddress))
                        Me.Elements.Add(ElementType.Folders, Generator.Trim(base.SelectSingleNode("Folders").InnerText))
                        Me.Elements.Add(ElementType.Files, Generator.Trim(base.SelectSingleNode("Files").InnerText))
                        For Each d As String In Directory.GetDirectories(Dir)
                            Dim current As New DirectoryInfo(d)
                            If (Me.Client.Settings.HideDotNames And current.Name.StartsWith(".")) Then
                                Continue For
                            End If
                            output.Add(String.Format(Me.Elements(ElementType.Folders), relativeAddress, current.Name, current.LastWriteTime.ToString("r")))
                        Next
                        For Each f As String In Directory.GetFiles(Dir)
                            Dim current As New FileInfo(f)
                            If (Not Me.Client.IsHiddenFileType(f)) Then
                                If (Me.Client.Settings.HideDotNames And current.Name.StartsWith(".")) Then
                                    Continue For
                                End If
                                If (current.Name.ToLower.Equals(Me.Elements(ElementType.Readme).ToLower)) Then
                                    Using sr As New StreamReader(New FileStream(f, FileMode.Open, FileAccess.Read, FileShare.None))
                                        Me.Elements(ElementType.Body) = Me.Elements(ElementType.Body).Replace(Me.Elements(ElementType.ReadmeTag), sr.ReadToEnd)
                                    End Using
                                End If
                                output.Add(String.Format(Me.Elements(ElementType.Files), relativeAddress, current.Name, current.LastWriteTime.ToString("r"), current.Length.HumanReadable))
                            End If
                        Next
                        '// Append to byte array
                        Me.AddRange(Me.Client.Encoding.GetBytes(Me.Elements(ElementType.Body).Replace("{TABLE}", String.Join(Environment.NewLine, output))))
                    End If
                End If
            End If
        End If
        Return Me.ToArray
    End Function
    ''' <summary>
    ''' Trims string data
    ''' </summary>
    Public Shared Function Trim(str As String) As String
        Return str.Trim(ControlChars.Tab, ControlChars.Cr, ControlChars.Lf)
    End Function
    ''' <summary>
    ''' Cast the first XML element by reference
    ''' </summary>
    Public Shared Function GetElement(node As XmlDocument, Reference As String) As XmlElement
        Return node.GetElementsByTagName(Reference).Cast(Of XmlElement)().First
    End Function
    ''' <summary>
    ''' Cast the first XML element by reference
    ''' </summary>
    Public Shared Function GetElement(node As XmlElement, Reference As String) As XmlElement
        Return node.GetElementsByTagName(Reference).Cast(Of XmlElement)().First
    End Function
    ''' <summary>
    ''' Validates XML document
    ''' </summary>
    Public Shared Function HasDefinedTemplate(node As XmlDocument) As Boolean
        Return node.SelectSingleNode("Templates") IsNot Nothing
    End Function
    ''' <summary>
    ''' Validates XML document
    ''' </summary>
    Public Shared Function HasDefinedErrorTemplate(node As XmlElement) As Boolean
        Return node.SelectSingleNode("ErrorTemplate") IsNot Nothing
    End Function
    ''' <summary>
    ''' Validates XML document
    ''' </summary>
    Public Shared Function HasDefinedReadMe(node As XmlElement) As Boolean
        Return node.SelectSingleNode("Readme") IsNot Nothing
    End Function
    ''' <summary>
    ''' Validates XML document
    ''' </summary>
    Public Shared Function HasDefinedDirTemplate(node As XmlElement) As Boolean
        Return node.SelectSingleNode("DirectoryTemplate") IsNot Nothing
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
