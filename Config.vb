Imports System.IO
Imports System.Xml
Imports System.Net
Imports System.Text
Imports System.Reflection
Imports System.Globalization
Imports Webserver.Plugins

Public MustInherit Class Config
#Region "Routines"
    ''' <summary>
    ''' Initializes the config class and returns true if its valid and supported
    ''' </summary>
    Public Function Initialize(BasePath As String) As Boolean
        Me.m_root = New DirectoryInfo(Path.GetFullPath(BasePath))
        If (HttpListener.IsSupported AndAlso Me.Valid) Then
            Me.Plugins = New List(Of IPlugin)
            Me.VirtualHosts = New List(Of VirtualHost)
            Me.ContentTypes = New Dictionary(Of String, String)
            Return True
        End If
        Return False
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
#Region "Properties"
    ''' <summary>
    ''' Holds the default fallback content type
    ''' </summary>
    Public Const DEFAULT_CONTENT_TYPE As String = "application/octet-stream"
    ''' <summary>
    ''' Holds the delay in msecs for the queue thread
    ''' </summary>
    Public Property Delay As Integer = 100
    ''' <summary>
    ''' Holds the total threadpool workers
    ''' </summary>
    Public Property Workers As Integer = 1
    ''' <summary>
    ''' Holds the total IO thread workers
    ''' </summary>
    Public Property IOWorkers As Integer = 1
    ''' <summary>
    ''' Holds the plugin assemblies if any
    ''' </summary>
    Public Property Plugins As List(Of IPlugin)
    ''' <summary>
    ''' Holds the vhirtual hosts definitions
    ''' </summary>
    Public Property VirtualHosts As List(Of VirtualHost)
    ''' <summary>
    ''' Holds the defined content types (MIME)
    ''' </summary>
    Public Property ContentTypes As Dictionary(Of String, String)
    ''' <summary>
    ''' Holds the defined time outs and rates for the httplistener
    ''' </summary>
    Public Property TimeOutDrainEntityBody As Integer
    Public Property TimeOutEntityBody As Integer
    Public Property TimeOutHeaderWait As Integer
    Public Property TimeOutIdleConnection As Integer
    Public Property TimeOutRequestPickup As Integer
    ''' <summary>
    ''' Holds the defined minimal send rate
    ''' </summary>
    Public Property MinSendBytesPerSecond As Integer
    ''' <summary>
    ''' Holds the defined maximum request queue
    ''' </summary>
    Public Property MaxWaitQueue As Integer
    ''' <summary>
    ''' Holds the defined boolean to show errors
    ''' </summary>
    Public Property ShowErrors As Boolean = False
    ''' <summary>
    ''' Holds the absolute base path of the server
    ''' </summary>
    Private m_root As DirectoryInfo
    Public ReadOnly Property Root As DirectoryInfo
        Get
            Return Me.m_root
        End Get
    End Property
    ''' <summary>
    ''' Returns basepath combined with given folder names
    ''' </summary>
    Public ReadOnly Property LocalPath(ParamArray Combine() As String) As String
        Get
            Return Path.GetFullPath(String.Format("{0}\{1}", Me.Root.FullName, String.Join("\", Combine)))
        End Get
    End Property
    ''' <summary>
    ''' Returns true if config has a valid base path
    ''' </summary>
    Public ReadOnly Property Valid As Boolean
        Get
            Return Me.Root IsNot Nothing AndAlso Me.Root.Exists
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
                base = Config.GetElement(document, "Config")
                listener = New Listener()
                If (listener.Initialize(base.GetAttribute("Base"))) Then
                    If (Config.HasDefinedThreadSettings(base)) Then
                        Integer.TryParse(base.GetAttribute("IO"), listener.IOWorkers)
                        Integer.TryParse(base.GetAttribute("Delay"), listener.Delay)
                        Integer.TryParse(base.GetAttribute("Workers"), listener.Workers)
                    End If
                    If (Config.HasDefinedListenerSettings(base)) Then
                        Dim settings As XmlElement = Config.GetElement(base, "Listener")
                        Integer.TryParse(settings.GetAttribute("DrainEntityBody"), listener.TimeOutDrainEntityBody)
                        Integer.TryParse(settings.GetAttribute("EntityBody"), listener.TimeOutEntityBody)
                        Integer.TryParse(settings.GetAttribute("HeaderWait"), listener.TimeOutHeaderWait)
                        Integer.TryParse(settings.GetAttribute("IdleConnection"), listener.TimeOutIdleConnection)
                        Integer.TryParse(settings.GetAttribute("RequestPickup"), listener.TimeOutRequestPickup)
                        Integer.TryParse(settings.GetAttribute("MinSendBytesPerSecond"), listener.MinSendBytesPerSecond)
                        Integer.TryParse(settings.GetAttribute("MaxWaitQueue"), listener.MaxWaitQueue)
                    End If
                    If (Config.HasDefinedPlugins(base)) Then
                        For Each plugin As String In Config.Parse(base.SelectSingleNode("Plugins").InnerText)
                            Dim fn As String = listener.LocalPath(plugin)
                            If (File.Exists(fn)) Then
                                If (Not Config.LoadAssembly(fn, listener.Plugins)) Then
                                    Throw New Exception(String.Format("Could not load plugin {0}", Path.GetFileName(fn)))
                                End If
                            End If

                        Next
                    End If
                    If (Config.HasDefinedDebugging(base)) Then
                        Dim debugging As XmlElement = Config.GetElement(base, "Debugging")
                        Boolean.TryParse(debugging.GetAttribute("ShowErrors"), listener.ShowErrors)
                    End If
                    If (Config.ValidateContentType(base)) Then
                        listener.ContentTypes = Config.Parse(base.SelectSingleNode("ContentType").InnerText, "=")
                        If (Config.HasDefinedVirtualHosts(base)) Then
                            For Each node As XmlElement In base.SelectNodes("VirtualHost")
                                Dim Settings As New VirtualHost
                                If (Settings.Initialize(listener, node.GetAttribute("Base"), node.GetAttribute("Prefix"), node.GetAttribute("Encoding"))) Then
                                    Boolean.TryParse(node.GetAttribute("KeepAlive"), Settings.KeepAlive)
                                    Boolean.TryParse(node.SelectSingleNode("HideDotNames").InnerText, Settings.HideDotNames)
                                    Integer.TryParse(node.SelectSingleNode("MaxQueryLength").InnerText, Settings.MaxQueryLength)
                                    Integer.TryParse(node.SelectSingleNode("MaxQueryVariableSize").InnerText, Settings.MaxQuerySize)
                                    Boolean.TryParse(node.SelectSingleNode("AllowDirListing").InnerText, Settings.AllowDirListing)
                                    Settings.AccessConfig = node.SelectSingleNode("AccessFile").InnerText
                                    Settings.Headers = VirtualHost.Parse(node.SelectSingleNode("CustomHeaders").InnerText, "=")
                                    Settings.HiddenFileTypes = VirtualHost.Parse(node.SelectSingleNode("HiddenFileTypes").InnerText)
                                    Settings.DefaultIndexPages = VirtualHost.Parse(node.SelectSingleNode("DefaultIndexPages").InnerText)
                                    Settings.ErrorPageTemplate = node.SelectSingleNode("ErrorPageTemplate").InnerText
                                    Settings.DirectoryTemplate = node.SelectSingleNode("DirectoryListingTemplate").InnerText
                                    Settings.IllegalPathChars = VirtualHost.Parse(node.SelectSingleNode("IllegalPathChars").InnerText)
                                    If (Config.HasDefinedLocalization(node)) Then
                                        Settings.Culture = New CultureInfo(node.SelectSingleNode("Culture").InnerText)
                                    End If
                                    listener.VirtualHosts.Add(Settings)
                                Else
                                    Throw New Exception("virtualhost Document root does not exist")
                                End If
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
                Throw New IOException("invalid xml config")
            End If
        End If
        Throw New IOException("config file not found")
    End Function
    ''' <summary>
    ''' Creates a new httplistener with defined prefix values
    ''' </summary>
    Public Shared Function Create(Settingss As List(Of VirtualHost)) As HttpListener
        Dim httplistener As New HttpListener
        For Each Settings As VirtualHost In Settingss
            httplistener.Prefixes.Add(String.Format("http://{0}/", Settings.Prefix))
        Next
        Return httplistener
    End Function
    ''' <summary>
    ''' Parses a string with CRLF as delimiter and returns a List(Of T)
    ''' </summary>
    Public Shared Function Parse(value As String) As List(Of String)
        Dim collection As New List(Of String)
        For Each line As String In Strings.Split(value, Environment.NewLine)
            If (line.Length > 0) Then
                collection.Add(line.Trim)
            End If
        Next
        Return collection
    End Function
    ''' <summary>
    ''' Parses a string with given delimiter and returns a dictonary
    ''' </summary>
    Public Shared Function Parse(value As String, delimiter As String) As Dictionary(Of String, String)
        Dim collection As New Dictionary(Of String, String)
        For Each line As String In Strings.Split(value, Environment.NewLine)
            If (line.Length > 0) Then
                If (line.Contains(delimiter)) Then
                    collection.Add(line.Substring(0, line.IndexOf(delimiter)).Trim, line.Substring(line.IndexOf(delimiter) + 1).Trim)
                End If
            End If
        Next
        Return collection
    End Function
    ''' <summary>
    ''' Returns plugin assembly from given filename
    ''' </summary>
    Public Shared Function LoadAssembly(filename As String, Collection As List(Of IPlugin)) As Boolean
        Try
            Dim asm As Assembly = Assembly.LoadFile(filename)
            For Each t As Type In asm.GetTypes.Where(Function(x) GetType(IPlugin).IsAssignableFrom(x))
                Collection.Add(CType(Activator.CreateInstance(t), IPlugin))
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
    ''' <summary>
    ''' Cast the first XML element by reference
    ''' </summary>
    Public Shared Function GetElement(node As XmlDocument, Reference As String) As XmlElement
        Return node.GetElementsByTagName(Reference).Cast(Of XmlElement)().First
    End Function
    Public Shared Function GetElement(node As XmlElement, Reference As String) As XmlElement
        Return node.GetElementsByTagName(Reference).Cast(Of XmlElement)().First
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
    Public Shared Function HasDefinedListenerSettings(node As XmlElement) As Boolean
        Return node.SelectSingleNode("Listener") IsNot Nothing
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
    Public Shared Function HasDefinedLocalization(node As XmlElement) As Boolean
        Return node.SelectSingleNode("Culture") IsNot Nothing
    End Function
    ''' <summary>
    ''' Validates XML element
    ''' </summary>
    Public Shared Function HasDefinedPlugins(node As XmlElement) As Boolean
        Return node.SelectSingleNode("Plugins") IsNot Nothing
    End Function
    ''' <summary>
    ''' Validates XML element
    ''' </summary>
    Public Shared Function HasDefinedDebugging(node As XmlElement) As Boolean
        Return node.SelectSingleNode("Debugging") IsNot Nothing
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
        Return node.SelectSingleNode("CustomHeaders") IsNot Nothing AndAlso
               node.SelectSingleNode("HiddenFileTypes") IsNot Nothing AndAlso
               node.SelectSingleNode("DefaultIndexPages") IsNot Nothing AndAlso
               node.SelectSingleNode("MaxQueryLength") IsNot Nothing AndAlso
               node.SelectSingleNode("MaxQueryVariableSize") IsNot Nothing AndAlso
               node.SelectSingleNode("AllowDirListing") IsNot Nothing AndAlso
               node.SelectSingleNode("HideDotNames") IsNot Nothing AndAlso
               node.SelectSingleNode("IllegalPathChars") IsNot Nothing AndAlso
               node.SelectSingleNode("ErrorPageTemplate") IsNot Nothing AndAlso
               node.SelectSingleNode("DirectoryListingTemplate") IsNot Nothing AndAlso
               node.SelectSingleNode("AccessFile") IsNot Nothing
    End Function
#End Region
End Class
