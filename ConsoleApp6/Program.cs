
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Text;

namespace ConsoleApp6
{
    public class Program
    {
        public static void Main(string[] args)
        {
            int random = new Random().Next(1, 1000);
            IConnectionFactory connFactory = new ConnectionFactory()
            {
                HostName = "127.0.0.1",//IP地址
                Port = 5672,//端口号
                UserName = "admin",//用户账号
                Password = "admin123",//用户密码
                VirtualHost = "TestRabbitMq"
            };
            using (IConnection conn = connFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    //交换机名称
                    String exchangeName = "cavalier_test";

                    //声明交换机
                    channel.ExchangeDeclare(exchange: exchangeName, type: "fanout");
                    // 消息队列名称
                    String queueName = exchangeName + "_" + random.ToString();

                    //声明队列
                    channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                    //将队列与交换机进行绑定
                    channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "");

                    ////告诉Rabbit每次只能向消费者发送一条信息,再消费者未确认之前,不再向他发送信息  
                    channel.BasicQos(0, 1, false);

                    //事件基本消费者
                    EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

                    //接收到消息事件
                    consumer.Received += (ch, ea) =>
                    {
                        var message = Encoding.UTF8.GetString(ea.Body);
                        Console.WriteLine($"收到消息： {message}");
                        //确认该消息已被消费
                        channel.BasicAck(ea.DeliveryTag, false);
                    };

                    //开启监听
                    channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
                    Console.ReadKey();
                }
            }
        }
    }
}

