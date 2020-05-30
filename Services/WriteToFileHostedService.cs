﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPITutorial.Services
{
    public class WriteToFileHostedService : IHostedService
    {
        private readonly IWebHostEnvironment env;
        private readonly string FileName = "File 1.txt";
        private Timer timer;
        public WriteToFileHostedService(IWebHostEnvironment env)
        {
            this.env = env;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            WriteToFile("** Process Started **");
            timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            WriteToFile(" ** Process Stopped **");
            return Task.CompletedTask;
        }
        private void DoWork(object value)
        {
            WriteToFile("** Process ongoing: " + DateTime.Now.ToString("dd/MM/YYYY hh:mm:ss"));
        }
        private void WriteToFile(string message)
        {
            var path = $@"{env.ContentRootPath}\MyLogFiles\{FileName}";
            using (StreamWriter writer = new StreamWriter(path, append: true))
            {
                writer.WriteLine(message);
            }
        }
    }
}
