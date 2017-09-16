Imports System.Net.Sockets
Imports System.Net
Imports System.Text
Imports System.Web.Script.Serialization

Public Class TCPCliente

    Private _socketTcpCliente As TcpClient
    Private _porta As Integer = 8000
    Private _IP As IPAddress = IPAddress.Parse("127.0.0.1")
    Private _networkStream As NetworkStream
    Private _serializer As JavaScriptSerializer
    Private _localVirtualTime As Integer

    Delegate Sub SetStatusCallback([text] As String)

    Private Sub TCPCliente_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            _serializer = New JavaScriptSerializer
            _localVirtualTime = 0
            _socketTcpCliente = New TcpClient()
            SetStatus("Estabelecendo conexão.")
            'cria um socket para este ip e porta
            _socketTcpCliente.Connect(_IP, _porta)
            'pega o stream entre o cliente e o servidor neste ip e porta
            _networkStream = _socketTcpCliente.GetStream()

            Dim dto As Models.DTO = LeMensagem()

            Me.Text = "Cliente " & dto.mensagem
            SetStatus("Conexão aceita...")

            _localVirtualTime = dto.timeStamp

            Dim ctThread As Threading.Thread = New Threading.Thread(AddressOf OuveRetornoServer)
            ctThread.Start()
        Catch ex As Exception
            'se n encontrou um servidor mostra uma mensagem e fecha o form
            Dim result = MessageBox.Show("Ocorreu um erro ao se conectar com o servidor!" & vbCrLf & ex.Message, "Atenção", MessageBoxButtons.OK)
            If result = DialogResult.OK Then
                Me.Close()
            End If
        End Try
    End Sub

    Private Sub TCPCliente_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        _SocketTcpCliente.Close()
        Environment.Exit(0)
    End Sub

    Private Sub txtMensagem_KeyDown(sender As Object, e As KeyEventArgs) Handles txtMensagem.KeyDown
        If e.KeyCode = Keys.KeyCode.Enter Then
            btnEnviar.PerformClick()
        End If
    End Sub

    Private Sub btnEnviar_Click(sender As Object, e As EventArgs) Handles btnEnviar.Click
        'verifica se pode ler e escrever no stream
        If _NetworkStream.CanWrite Then
            EnviaMensagem(txtMensagem.Text)
            txtMensagem.Text = ""
        Else
            SetStatus("Não é possivel escrever dados neste stream")
            _SocketTcpCliente.Close()
        End If
    End Sub

    Private Sub SetStatus(ByVal text As String)
        If Me.txtStatus.InvokeRequired Then
            Dim d As New SetStatusCallback(AddressOf SetStatus)
            Me.Invoke(d, New Object() {text})
        Else
            Dim texto = ""
            If (Not String.IsNullOrEmpty(Me.txtStatus.Text)) Then
                texto &= vbCrLf
            End If
            texto &= text
            Me.txtStatus.AppendText(texto)
        End If
    End Sub

    Private Sub OuveRetornoServer()
        While True
            If _NetworkStream.CanRead Then
                Dim dto As Models.DTO = LeMensagem()
                _localVirtualTime = dto.timeStamp
                SetStatus("[" & dto.timeStamp & "] " & dto.mensagem)
            Else
                SetStatus("Não é possivel ler dados deste stream")
                _SocketTcpCliente.Close()
            End If
        End While
    End Sub

    Private Sub EnviaMensagem(ByVal mensagem As String)
        _localVirtualTime += 1
        ' cria objeto para mandar
        Dim dto As New Models.DTO
        dto.timeStamp = _localVirtualTime
        dto.mensagem = mensagem
        'serializa objeto
        Dim resultado As String = _serializer.Serialize(dto)
        Dim sendBytes As [Byte]() = Encoding.UTF8.GetBytes(resultado)
        _networkStream.Write(sendBytes, 0, sendBytes.Length)
    End Sub

    Private Function LeMensagem() As Models.DTO
        Dim bytes(_socketTcpCliente.ReceiveBufferSize) As Byte
        _networkStream.Read(bytes, 0, CInt(_socketTcpCliente.ReceiveBufferSize))
        Dim returndata As String = Encoding.UTF8.GetString(bytes)
        Dim dto As Models.DTO = _serializer.Deserialize(Of Models.DTO)(returndata.Replace(vbNullChar, ""))
        Return dto
    End Function

End Class
