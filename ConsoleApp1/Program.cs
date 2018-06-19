using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ConsoleApp1;
using Newtonsoft.Json.Linq;
public class SynchronousSocketClient
{

    private static String WIN_RESULT = "win";
    private static String LOSE_RESULT = "lose";
    private static String MY_SIGN = "O";
    private static String OPONENT_SIGN = "X";
    private static String[,] border = null;
    private static bool WIN_GAME = false;
    private static int emptyField = 0;
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

                JObject json = JObject.Parse(data);
                int player = json["player"].Value<int>();
                int borderSize = json["size"].Value<int>();
                Console.WriteLine("Player: " + player);
                Console.WriteLine("Size: " + borderSize);
                border = new string[borderSize, borderSize];
                Console.ReadLine();
                while (data != WIN_RESULT || data != LOSE_RESULT)
                {
                    JObject jsons;
                    int x, y;
                    byte[] msg = null;
                    if (player.Equals(0))
                    {
                        msg = sendPosition(msg);
                        sender.Send(msg);
                        Console.WriteLine("Send!. Press Enter to continue...");
                        Console.ReadLine();

                        bytesRec = sender.Receive(bytes);
                        data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        jsons = JObject.Parse(data);
                        x = jsons["x"].Value<int>();
                        y = jsons["y"].Value<int>();
                      
                        setMoveToCopyServerBoard(x, y,  OPONENT_SIGN);
                        Console.WriteLine("Recived: " + data);
                    }
                    else
                    {
                        bytesRec = sender.Receive(bytes);
                        data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        jsons = JObject.Parse(data);
                        x = jsons["x"].Value<int>();
                        y = jsons["y"].Value<int>();
                        setMoveToCopyServerBoard(x, y, OPONENT_SIGN);
                        Console.WriteLine("Recived: " + data);

                        msg = sendPosition(msg);
                        sender.Send(msg);
                        Console.WriteLine("Send!. Press Enter to continue...");
                        Console.ReadLine();
                    }

                    
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

    private static void setMoveToCopyServerBoard(int x, int y,String sign)
    {
                border[x,y] = sign;   
    }

    private static byte[] sendPosition(byte[] msg)
    {
        msg = checkFieldAndSetNewField(border);

        return msg;
    }

    private static byte[] checkFieldAndSetNewField(string[,] border)
    {
     
        byte[] msg = null;
        Console.WriteLine("SIZE OF BORDER " + border.GetLength(0));
        int m = minMax(border, MY_SIGN); 
        Position p = botMove(border);
        msg = Encoding.ASCII.GetBytes(@"{x:" + p.x + ",y:" + p.y + "}<EOF>");
        /* if (border[0, 0] == null) 
         {
             msg = Encoding.ASCII.GetBytes(@"{x:"+0+",y:"+0+"}<EOF>");

         }
         else
         {
             msg = Encoding.ASCII.GetBytes(@"{x:" + 2 + ",y:" + 2 + "}<EOF>");
         }
         Console.WriteLine("Send: " + Encoding.ASCII.GetString(msg));*/
        return msg;
    }
    private static Position botMove(string[,] border) // to ma wysyłać ruch
    {
        Position position = new Position();
        int move, m, mmx;
        mmx = -10;
        for (int i = 0; i < border.GetLength(0) - 1; i++)
        {
            for (int j = 0; j < border.GetLength(1) - 1; j++)
            {
                if (border[i, j].Equals(null))
                {
                    border[i, j] = MY_SIGN;
                    m = minMax(border, MY_SIGN);
                    border[i, j] = null;
                    if(m > mmx)
                    {
                        mmx = m;
                        position.x = i;
                        position.y = j;
                    }

                }
            }
        }
        return position;
    }
    private static int minMax(string[,] border, string mY_SIGN)
    {
        int m=0, mmx = 0;

        if (win_game())
        {
            m = 1;
        }else if (draw())
        {
            m = 0;
        }
        else
        {
            m = -1;
        }
        mY_SIGN = (mY_SIGN == "X") ? "O" : "X";
        mmx = (mY_SIGN == "O") ? 10 : -10;

        for(int i = 0; i < border.GetLength(0) - 1; i++)
        {
            for(int j=0;j<border.GetLength(1) - 1; j++)
            {
                if (border[i, j] == null)
                {
                    border[i, j] = mY_SIGN;
                    m = minMax(border, mY_SIGN);
                    border[i, j] = null;
                    if (((mY_SIGN == "O") && (m < mmx)) || ((mY_SIGN == "X") && (m > mmx))) mmx = m;
                }
            }
        }

        return mmx;
    }

    private static bool draw()
    {
        bool draw = false;
        if (checkEmptyField(border) == 0)
        {
            if (WIN_GAME)
            {
                draw = true;
            }
          
        }
        return draw;
    }
    private static int checkEmptyField(string[,] border)
    {
        
        for (int i = 0; i < border.GetLength(0) -1; i++)
        {
            for (int j = 0; j < border.GetLength(0) -1; j++)
            {
                if (border[i, j] == null)
                {
                    emptyField++;
                }
            }
        }
        return emptyField;
    }
    private static bool win_game()
    {
        
        for (int i = 0; i < border.GetLength(0) - 1; i++)
        {
            for(int j=0;j < border.GetLength(1) -1; j++)
            {
                if(border[i,j] == MY_SIGN)
                {
                    WIN_GAME = true;
                }
            }
        }

        for(int i=0;i<border.GetLength(0) -1; i++)
        {
            if(border[0,i] == MY_SIGN)
            {
                WIN_GAME = true;
            }
        }

        for (int i = 0; i < border.GetLength(1) - 1; i++)
        {
            if (border[i, 0] == MY_SIGN)
            {
                WIN_GAME = true;
            }
        }

            return WIN_GAME;
    }

    public static int Main(String[] args)
    {
        StartClient();
        return 0;
    }
}