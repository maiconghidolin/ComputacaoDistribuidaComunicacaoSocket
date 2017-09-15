Imports System.Net.Sockets
Imports System.Net
Imports System.Text

Public Class TCPCliente

    Private _SocketTcpCliente As TcpClient
    Private _Porta As Integer = 8000
    Private _IP As IPAddress = IPAddress.Parse("127.0.0.1")
    Private _NetworkStream As NetworkStream

    Delegate Sub SetStatusCallback([text] As String)

    Private Sub TCPCliente_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            _SocketTcpCliente = New TcpClient()
            SetStatus("Estabelecendo conexão.")
            'cria um socket para este ip e porta
            _SocketTcpCliente.Connect(_IP, _Porta)
            'pega o stream entre o cliente e o servidor neste ip e porta
            _NetworkStream = _SocketTcpCliente.GetStream()

            Dim bytes(_SocketTcpCliente.ReceiveBufferSize) As Byte
            _NetworkStream.Read(bytes, 0, CInt(_SocketTcpCliente.ReceiveBufferSize))
            Dim returndata As String = Encoding.UTF8.GetString(bytes)
            Me.Text = "Cliente " & returndata
            SetStatus("Conexão aceita...")

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

    Private Sub btnEnviar_Click(sender As Object, e As EventArgs) Handles btnEnviar.Click
        'verifica se pode ler e escrever no stream
        If _NetworkStream.CanWrite Then
            ' envia alguma coisa p server
            Dim resultado As String = txtMensagem.Text
            Dim sendBytes As [Byte]() = Encoding.UTF8.GetBytes(resultado)
            _NetworkStream.Write(sendBytes, 0, sendBytes.Length)
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
                ' Le o retorno do server
                Dim bytes(_SocketTcpCliente.ReceiveBufferSize) As Byte
                _NetworkStream.Read(bytes, 0, CInt(_SocketTcpCliente.ReceiveBufferSize))

                ' exibe os dados recebidos do server
                Dim returndata As String = Encoding.UTF8.GetString(bytes)
                SetStatus(returndata)
            Else
                SetStatus("Não é possivel ler dados deste stream")
                _SocketTcpCliente.Close()
            End If
        End While
    End Sub

End Class
