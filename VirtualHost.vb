﻿Imports System.IO
Imports System.Text

Public Class VirtualHost
#Region "Constructors"
    Sub New(Parent As Config, DocumentRoot As String, Prefix As String, Encoder As String)
        Me.Parent = Parent
        Me.Prefix = Prefix
        Me.Encoder = Encoding.GetEncoding(Encoder)
        Me.HiddenFileTypes = New List(Of String)
        Me.DefaultIndexPages = New List(Of String)
        Me.IllegalPathChars = New List(Of String)
        Me.Headers = New Dictionary(Of String, String)
        Me.DocumentRoot = VirtualHost.GetDirectory(Parent.BasePath.FullName, DocumentRoot)
    End Sub
#End Region
#Region "Shared"
    Public Shared Function Parse(value As String) As List(Of String)
        Dim collection As New List(Of String)
        For Each line As String In Strings.Split(value, Environment.NewLine)
            collection.Add(line.Trim)
        Next
        Return collection
    End Function
    ''' <summary>
    ''' Parses a string with given delimiter and returns a dictonary
    ''' </summary>
    Public Shared Function Parse(value As String, delimiter As String) As Dictionary(Of String, String)
        Dim collection As New Dictionary(Of String, String)
        For Each line As String In Strings.Split(value, Environment.NewLine)
            If (line.Contains(delimiter)) Then
                collection.Add(line.Substring(0, line.IndexOf(delimiter)).Trim, line.Substring(line.IndexOf(delimiter) + 1).Trim)
            End If
        Next
        Return collection
    End Function
    ''' <summary>
    ''' Returns DirectoryInfo from given base path
    ''' </summary>
    Public Shared Function GetDirectory(BasePath As String, DocumentRoot As String) As DirectoryInfo
        Return New DirectoryInfo(Path.GetFullPath(String.Format("{0}\{1}", BasePath, DocumentRoot)))
    End Function
#End Region
#Region "Properties"
    Public Property Parent As Config
    Public Property Prefix As String
    Public Property Encoder As Encoding
    Public Property KeepAlive As Boolean
    Public Property ErrorPage As String
    Public Property DocumentRoot As DirectoryInfo
    Public Property MaxQuerySize As Integer = 255
    Public Property MaxQueryLength As Integer = 255
    Public Property DirectoryTemplate As String
    Public Property HiddenFileTypes As List(Of String)
    Public Property IllegalPathChars As List(Of String)
    Public Property DefaultIndexPages As List(Of String)
    Public Property HideDotNames As Boolean = True
    Public Property AllowDirListing As Boolean = False
    Public Property Headers As Dictionary(Of String, String)
#End Region
#Region "Read Only Properties"
    Public ReadOnly Property Name As String
        Get
            Return Me.Prefix.Substring(0, Me.Prefix.IndexOf(":")).ToLower
        End Get
    End Property
    Public ReadOnly Property Validated As Boolean
        Get
            Return Me.DocumentRoot IsNot Nothing AndAlso Me.DocumentRoot.Exists
        End Get
    End Property
#End Region
End Class
