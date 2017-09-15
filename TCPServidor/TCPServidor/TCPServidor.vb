Imports System.Net.Sockets
Imports System.Text
Imports System.Net

Public Class TCPServidor

    Private _socketTcpCliente As TcpClient
    Private _porta As Integer = 8000
    Private _IP As IPAddress = IPAddress.Parse("127.0.0.1")
    Private _tcpListener As TcpListener
    Private _contador As Integer
    Private _contadorAux As Integer
    Private _listaClientes As List(Of Object)

    Delegate Sub SetTextoCallback([text] As String)

    Private Sub TCPServidor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            _listaClientes = New List(Of Object)

            ' cria um ouvinte nesta porta e ip
            _tcpListener = New TcpListener(_IP, _porta)
            _tcpListener.Start()
            Me.SetTexto("Servidor iniciado...")

            'cria thread que vai ficar esperando conexoes dos clientes
            Dim ctThread As Threading.Thread = New Threading.Thread(AddressOf VerificaConexaoCliente)
            ctThread.Start()
        Catch ex As SocketException
            'se a porta ja esta em uso
            Dim result = MessageBox.Show(ex.Message, "Atenção", MessageBoxButtons.OK)
            If result = DialogResult.OK Then
                Me.Close()
            End If
        End Try
    End Sub

    Private Sub TCPServidor_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        _tcpListener.Stop()
        Environment.Exit(0)
    End Sub

    Private Sub VerificaConexaoCliente()
        _contador = 1
        While True
            'aceita a conexao do cliente e retorna uma inicializacao TcpClient 
            'fica aqui ate receber uma conexão de um cliente
            _socketTcpCliente = _tcpListener.AcceptTcpClient()
            Me.SetTexto(vbCrLf & "Conexão com o cliente " & _contador & " aceita...")
            _listaClientes.Add(New With {.Id = _contador, .Socket = _socketTcpCliente})

            'cria uma thread para cada cliente onde vai ficar ouvindo a conexao 
            'se o cliente enviar alguma coisa a thread responde
            _contadorAux = _contador
            Dim ctThread As Threading.Thread = New Threading.Thread(AddressOf OuveCliente)
            ctThread.Start()

            _contador += 1
        End While
    End Sub

    Private Sub OuveCliente()
        Dim contador = _contadorAux
        Dim socket = _socketTcpCliente

        ' obtem oq foi enviado pelo cliente
        Dim networkStream As NetworkStream = socket.GetStream()

        'retorna para o cliente o numero dele
        Dim resultado As String = contador.ToString
        Dim sendBytes As [Byte]() = Encoding.UTF8.GetBytes(Resultado)
        networkStream.Write(sendBytes, 0, sendBytes.Length)

        While True
            Try
                ' le o stream em um array de bytes
                Dim bytes(socket.ReceiveBufferSize) As Byte
                networkStream.Read(bytes, 0, CInt(socket.ReceiveBufferSize))

                If socket.Connected Then

                End If

                ' joga num campo oq foi mandado pelo cliente
                Dim clientdata As String = Encoding.UTF8.GetString(bytes)
                Me.SetTexto(vbCrLf & "Cliente " & contador & " enviou : " + clientdata)

                For Each cliente In _listaClientes
                    Dim retorno = contador & ": " & clientdata
                    sendBytes = Encoding.UTF8.GetBytes(retorno)
                    cliente.Socket.GetStream().Write(sendBytes, 0, sendBytes.Length)
                Next
            Catch ex As Exception
                'se o cliente encerrou
                Me.SetTexto(vbCrLf & "Conexão com o cliente " & contador & " encerrada...")
                _listaClientes = _listaClientes.Where(Function(x) x.Id <> contador).ToList
                'fecha o socket do cliente
                socket.Close()
                Exit While
            End Try
        End While
    End Sub

    Private Sub SetTexto(ByVal text As String)
        If Me.txtTexto.InvokeRequired Then
            Dim d As New SetTextoCallback(AddressOf SetTexto)
            Me.Invoke(d, New Object() {text})
        Else
            Dim texto = ""
            If (Not String.IsNullOrEmpty(Me.txtTexto.Text)) Then
                texto &= vbCrLf
            End If
            texto &= text
            Me.txtTexto.AppendText(texto)
        End If
    End Sub

End Class
