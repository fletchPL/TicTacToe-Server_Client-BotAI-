using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;
public class SynchronousSocketClient
{

    private static String WIN_RESULT = "win";
    private static String LOSE_RESULT = "lose";
    private bool PLAYER_0 = false;
    private bool PLAYER_1 = false;
    public static void StartClient()
    {
        // Data buffer for incoming data.  
        byte[] bytes = new byte[1024];

        // Connect to a remote device.  
        try
        {
            // Establish the remote endpoint for the socket.  
            // This example uses port 11000 on the local computer.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP  socket.  
            Socket sender = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.  
            try
            {
                sender.Connect(remoteEP);

                Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());


                string data = null;
                // Receive the response from the remote device.  
                int bytesRec = sender.Receive(bytes);
                data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                Console.WriteLine(data);
                
                Console.ReadLine();
                JObject json = JObject.Parse(data);
                int player = json["player"].Value<int>();
                Console.WriteLine("JSON: " + player);

                while (data != WIN_RESULT || data != LOSE_RESULT)
                {
                    Console.ReadLine();
                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes(@"{x:1,y:1}<EOF>");

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);

                    bytesRec = sender.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    Console.WriteLine(data);
                }
                // Release the socket.  
               sender.Shutdown(SocketShutdown.Both);
                sender.Close();


                Console.WriteLine("Outcome: {0}", data);
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static int Main(String[] args)
    {
        StartClient();
        return 0;
    }
}