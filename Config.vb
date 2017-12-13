Imports System.IO
Imports System.Xml
Imports System.Net
Imports System.Text

Public MustInherit Class Config
    Public Const DEFAULT_CONTENT_TYPE As String = "application/octet-stream"
    Public Property Delay As Integer = 100
    Public Property Workers As Integer = 1
    Public Property IOWorkers As Integer = 1
    Public Property VirtualHosts As List(Of VirtualHost)
    Public Property ContentTypes As Dictionary(Of String, String)
#Region "Routines"
    ''' <summary>
    ''' Initializes the config class and returns true if its valid and supported
    ''' </summary>
    Public Function Initialize(BasePath As String) As Boolean
        If (BasePath.Length > 0) Then
            Me.VirtualHosts = New List(Of VirtualHost)
            Me.ContentTypes = New Dictionary(Of String, String)
            Me.m_basepath = New DirectoryInfo(Path.GetFullPath(BasePath))
        End If
        Return Me.Validated
    End Function
    ''' <summary>
    ''' Tries to get the content type value for given filename
    ''' </summary>
    Public Function GetContentType(filename As FileInfo) As String
        Dim value As String = String.Empty
        If (Me.ContentTypes.TryGetValue(filename.Extension, value)) Then
            Return value
        End If
        Return Config.DEFAULT_CONTENT_TYPE
    End Function
    ''' <summary>
    ''' Tries to get the content type value for given filename
    ''' </summary>
    Public Function GetContentType(filename As String) As String
        Dim value As String = String.Empty
        If (Me.ContentTypes.TryGetValue(Path.GetExtension(filename), value)) Then
            Return value
        End If
        Return Config.DEFAULT_CONTENT_TYPE
    End Function
#End Region
#Region "Read Only Properties"
    ''' <summary>
    ''' Holds the absolute base path of the server
    ''' </summary>
    Private m_basepath As DirectoryInfo
    Public ReadOnly Property BasePath As DirectoryInfo
        Get
            Return Me.m_basepath
        End Get
    End Property
    ''' <summary>
    ''' Returns true if config has a valid base path and supported listener
    ''' </summary>
    Public ReadOnly Property Validated As Boolean
        Get
            Return Me.BasePath IsNot Nothing AndAlso Me.BasePath.Exists AndAlso HttpListener.IsSupported
        End Get
    End Property
#End Region
#Region "Shared"
    ''' <summary>
    ''' Parses XML config and returns a new listener class with these settings
    ''' </summary>
    Public Shared Function Load(filename As String) As Listener
        If (File.Exists(filename)) Then
            Dim document As New XmlDocument, listener As Listener, base As XmlElement
            Using fs As New FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None)
                document.Load(fs)
            End Using
            If (Config.ValidateDocument(document)) Then
                base = document.GetElementsByTagName("Config").Cast(Of XmlElement)().First
                Listener = New Listener()
                If (Listener.Initialize(base.GetAttribute("Base"))) Then
                    If (Config.HasDefinedThreadSettings(base)) Then
                        Integer.TryParse(base.GetAttribute("IO"), Listener.IOWorkers)
                        Integer.TryParse(base.GetAttribute("Delay"), Listener.Delay)
                        Integer.TryParse(base.GetAttribute("Workers"), Listener.Workers)
                    End If
                    If (Config.ValidateContentType(base)) Then
                        Listener.ContentTypes = Config.Parse(base.SelectSingleNode("ContentType").InnerText, "=")
                        If (Config.HasDefinedVirtualHosts(base)) Then
                            For Each node As XmlElement In base.SelectNodes("VirtualHost")
                                Dim vhost As New VirtualHost(Listener,
                                                             node.GetAttribute("Base"),
                                                             node.GetAttribute("Prefix"),
                                                             node.GetAttribute("Encoding"))
                                vhost.Headers = VirtualHost.Parse(node.SelectSingleNode("Headers").InnerText, "=")
                                vhost.HiddenFileTypes = VirtualHost.Parse(node.SelectSingleNode("HiddenFileTypes").InnerText)
                                vhost.DefaultIndexPages = VirtualHost.Parse(node.SelectSingleNode("DefaultIndexPages").InnerText)
                                Boolean.TryParse(node.GetAttribute("KeepAlive"), vhost.KeepAlive)
                                Boolean.TryParse(node.SelectSingleNode("HideDotNames").InnerText, vhost.HideDotNames)
                                Integer.TryParse(node.SelectSingleNode("MaxQueryLength").InnerText, vhost.MaxQueryLength)
                                Integer.TryParse(node.SelectSingleNode("MaxQueryVariableSize").InnerText, vhost.MaxQuerySize)
                                Boolean.TryParse(node.SelectSingleNode("AllowDirListing").InnerText, vhost.AllowDirListing)
                                vhost.ErrorPageTemplate = node.SelectSingleNode("ErrorPageTemplate").InnerText
                                vhost.DirectoryTemplate = node.SelectSingleNode("DirectoryListingTemplate").InnerText
                                vhost.IllegalPathChars = VirtualHost.Parse(node.SelectSingleNode("IllegalPathChars").InnerText)
                                listener.VirtualHosts.Add(vhost)
                            Next
                        Else
                            Throw New Exception("incomplete virtualhost config entry")
                        End If
                        Return listener
                    Else
                        Throw New Exception("config base directory not set")
                    End If
                Else
                    Throw New Exception("could not create server")
                End If
            Else
                Throw New Exception("invalid xml config")
            End If
        End If
        Throw New IOException("config file not found")
    End Function
    ''' <summary>
    ''' Creates a new httplistener with defined prefix values
    ''' </summary>
    Public Shared Function Create(vhosts As List(Of VirtualHost)) As HttpListener
        Dim httplistener As New HttpListener
        For Each vhost As VirtualHost In vhosts
            httplistener.Prefixes.Add(String.Format("http://{0}/", vhost.Prefix))
        Next
        Return httplistener
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
    ''' Validates XML document
    ''' </summary>
    Public Shared Function ValidateDocument(doc As XmlDocument) As Boolean
        Return doc.SelectSingleNode("Config") IsNot Nothing
    End Function
    ''' <summary>
    ''' Validates XML element
    ''' </summary>
    Public Shared Function HasDefinedVirtualHosts(node As XmlElement) As Boolean
        Return node.SelectSingleNode("VirtualHost") IsNot Nothing
    End Function
    ''' <summary>
    ''' Validates XML element
    ''' </summary>
    Public Shared Function HasDefinedThreadSettings(node As XmlElement) As Boolean
        Return node.SelectSingleNode("Threads") IsNot Nothing AndAlso
               node.HasAttribute("Workers") AndAlso node.HasAttribute("IO")
    End Function
    ''' <summary>
    ''' Validates XML element
    ''' </summary>
    Public Shared Function ValidateContentType(node As XmlElement) As Boolean
        Return node.SelectSingleNode("ContentType") IsNot Nothing
    End Function
    ''' <summary>
    ''' Validates XML element
    ''' </summary>
    Public Shared Function ValidateVirtualHost(node As XmlElement) As Boolean
        Return node.SelectSingleNode("Headers") IsNot Nothing AndAlso
               node.SelectSingleNode("HiddenFileTypes") IsNot Nothing AndAlso
               node.SelectSingleNode("DefaultIndexPages") IsNot Nothing AndAlso
               node.SelectSingleNode("MaxQueryLength") IsNot Nothing AndAlso
               node.SelectSingleNode("MaxQueryVariableSize") IsNot Nothing AndAlso
               node.SelectSingleNode("AllowDirListing") IsNot Nothing AndAlso
               node.SelectSingleNode("HideDotNames") IsNot Nothing AndAlso
               node.SelectSingleNode("IllegalPathChars") IsNot Nothing AndAlso
               node.SelectSingleNode("ErrorPageTemplate") IsNot Nothing AndAlso
               node.SelectSingleNode("DirectoryListingTemplate") IsNot Nothing
    End Function
#End Region
End Class
