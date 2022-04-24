using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TCWeb.Models;
using System.Data.OleDb;
using System.Data;
using System.IO;
using ExcelDataReader;

namespace TCWeb.Classes {
    internal interface IScopedProcessingService {
        Task DoWork(CancellationToken stoppingToken);
    }
    /// <summary>
    /// Clase para automatizar la replicación de datos, está se realiza cada 30 segundos.
    /// </summary>
    internal class TCScopedService : IScopedProcessingService {
        private int executionCount = 0;
        private readonly ILogger _logger;
        private HttpClient client;

        public TCScopedService(ILogger<TCScopedService> logger) {
            _logger = logger;
            client = new HttpClient();
        }

        public async Task DoWork(CancellationToken stoppingToken) {
            while (!stoppingToken.IsCancellationRequested) {
                executionCount++;

                var res = ReplicationServiceAsync();
                await Task.Delay(30000, stoppingToken);
            }
        }

        /// <summary>
        /// Función para replicar los datos tanto de la nube como del archivo local hacia otra base de datos en la nube.
        /// Solo copia o modifica registros no duplicados en las 3 bases de datos.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ReplicationServiceAsync() {
            var json = await client.GetStringAsync("http://ffalling-001-site1.itempurl.com/api/BBDD/GetCandidatos");
            var lista = JsonConvert.DeserializeObject<List<Item>>(json);


            var json2 = await client.GetStringAsync("http://ffalling-001-site1.itempurl.com/api/BBDD/GetCandidatosCloud");
            var listaCloud = JsonConvert.DeserializeObject<List<Item>>(json2);

            var res = lista.Where(i => (listaCloud.Where(x => x.Nombre == i.Nombre
                                                                && x.URL == i.URL
                                                                && x.Puesto == i.Puesto
                                                                && x.Numero == i.Numero
                                                                && x.Correo == i.Correo).FirstOrDefault() == null)).ToList();
            if(res.Count > 0) {
                //Revisa si es agregar o actualizar registro
                for(int i = 0; i < res.Count; i++) {
                    int nCloud = listaCloud.Where(x => x.Nombre == res[i].Nombre).Count();
                    if(nCloud > 0) {
                        var index = listaCloud.FindIndex(x => x.Nombre == res[i].Nombre) + 1;
                        var data = JsonConvert.SerializeObject(res[i]);
                        HttpContent content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                        var response = await client.PutAsync($"http://ffalling-001-site1.itempurl.com/api/BBDD/UpdateCandidato/{index}", content);
                    } else {
                        var data = JsonConvert.SerializeObject(res[i]);
                        HttpContent content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                        var response = await client.PostAsync("http://ffalling-001-site1.itempurl.com/api/BBDD/PostCandidatosCloud", content);
                    }  
                }
            }

            try {
                List<Item> localList = new List<Item>();
                using (var stream = new FileStream("BBDDCandidatos.xlsx", FileMode.Open, FileAccess.Read)) {
                    using (var reader = ExcelReaderFactory.CreateReader(stream)) {
                        var result = reader.AsDataSet();
                        var sd = result.Tables[0].Rows[0];
                        foreach (DataRow row in result.Tables[0].Rows) {
                            localList.Add(new Item() {
                                Nombre = row.ItemArray[0].ToString(),
                                URL = row.ItemArray[1].ToString(),
                                Puesto = row.ItemArray[2].ToString(),
                                Numero = row.ItemArray[3].ToString(),
                                Correo = row.ItemArray[4].ToString(),
                            });
                        }
                    }
                }

                var res2 = localList.Where(i => (listaCloud.Where(x => x.Nombre == i.Nombre
                                                                    && x.URL == i.URL
                                                                    && x.Puesto == i.Puesto
                                                                    && x.Numero == i.Numero
                                                                    && x.Correo == i.Correo).FirstOrDefault() == null)).ToList();
                if (res2.Count > 0) {
                    //Revisa si es agregar o actualizar registro
                    for (int i = 0; i < res2.Count; i++) {
                        int nCloud = listaCloud.Where(x => x.Nombre == res2[i].Nombre).Count();
                        if (nCloud > 0) {
                            var index = listaCloud.FindIndex(x => x.Nombre == res2[i].Nombre) + 1;
                            var data = JsonConvert.SerializeObject(res2[i]);
                            HttpContent content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                            var response = await client.PutAsync($"http://ffalling-001-site1.itempurl.com/api/BBDD/UpdateCandidato/{index}", content);
                        } else {
                            var data = JsonConvert.SerializeObject(res2[i]);
                            HttpContent content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                            var response = await client.PostAsync("http://ffalling-001-site1.itempurl.com/api/BBDD/PostCandidatosCloud", content);
                        }
                    }
                }
            } catch(Exception ex) {
                _logger.LogInformation("Error en archivo local, se omitió la copia a la nube.");
            }
            _logger.LogInformation("Diferences {Cout}", res.Count());
            return true;
        }
    }
}