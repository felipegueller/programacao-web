using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


class ServidorHttp 
{
    // A propriedade Controlador do tipo TCP listenner, vai receber um objeto do tipo TcpListener, quem tem como função ficar ouvindo as requisições TCP;
    private TcpListener Controlador {get; set; }
    // Número da porta que será escutada, por padrão vamos usar a localhost:8080
    private int Porta {get; set; }
    // QtdRequests é apenas um contador que ajudará ver se alguma requisição está sendo perdida
    private int QtdRequests {get; set; }

    public ServidorHttp (int porta = 8080)
    {   // atributo porta recebe como parâmetro opcional 8080
        this.Porta = porta;
        try
        {
            // cria um obejeto no qual inicia a escuta na porta 8080 através do endereço IP passado
            this.Controlador = new TcpListener(IPAddress.Parse("127.0.0.1"), this.Porta);
            this.Controlador.Start(); // inicia o servidor
            Console.WriteLine($"Servidor HTTP está rodando na porta { this.Porta }.");
            Console.WriteLine($"Para acessar, digite no navegador: http://localhost:{this.Porta}.");
            
            // Faz a chamada assíncrona do método AguardarRequests()
            Task ServidorHttpTask = Task.Run(() => AguardarRequests());

            // Apenas espera o fim da execução do método AguardarRequests
            ServidorHttpTask.GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Erro ao iniciar o servidor na porta {this.Porta}:\n{e.Message}");
        }
    }

    private async Task AguardarRequests()
    {
        // Aguarda a chegada de uma nova requisição em looping infinito
        while(true)
        {
            // AcceptSocketAsync() -> Aguarda a chegada de uma nova requisição e quando encontra, retorna um objeto do tipo sokect
            Socket conexao = await this.Controlador.AcceptSocketAsync();
            this.QtdRequests++; // adiciona +1 requisição

            // Chamada assíncrona por meio de tasks para o método Processar Requests 
            Task task = Task.Run(() => ProcessarRequest(conexao, this.QtdRequests));
        }
    }

    private void ProcessarRequest (Socket conexao, int numeroRequest)
    {
        Console.WriteLine($"Processando request #{numeroRequest}...\n");

        if(conexao.Connected) // Se a conexão for válida
        {
            // este vetor irá armazenar os bytes da requisição estabelecida
            byte[] bytesRequisicao = new byte[1024];

            // O método receive irá preencher o vetor de bytes
            // Parâmetros: 
            //      1° - indica onde você deseja armazenar o valor da requisição;
            //      2° - indica a quantidade de bytes que você deseja ler;
            //      3° - indica a posição que você deseja começar a leitura.
            conexao.Receive(bytesRequisicao, bytesRequisicao.Length, 0);

            // Encoding.UTF8.GetString(bytesRequisicao) -> converte os caracteres recebidos em bytesRequisicao para o formato string no padrão de código UTF-8 (acentuação)
            //.Replace((char)(0), ' ') -> troca o caracteres da string correspondente a 0 (zero) por um espaço, isso ocorre porque geralmente as requisições não ocupam todo o array de bytes, e assim as posições restantes são preenchidas com zeros.
            // Trim() -> elimina todos os espaços da string
            string textoRequisição = Encoding.UTF8.GetString(bytesRequisicao)
                    .Replace((char)(0), ' ').Trim();

            if(textoRequisição.Length > 0)
            {
                // Se o texto obtido for maior que zero, mostra o conteúdo da requisição e fecha a conexão
               Console.WriteLine($"\n{textoRequisição}");
               conexao.Close(); 
            } 
            Console.WriteLine($"\nRequest { numeroRequest } foi finalizado!!");
        }
    }
}