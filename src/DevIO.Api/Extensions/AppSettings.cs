using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.Extensions
{
    public class AppSettings
    {
        /// <summary>
        /// A chave de criptografia do JWT
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// Quantas horas o JWT irá levar até perder a validade
        /// </summary>
        public int ExpiracaoHoras { get; set; }

        /// <summary>
        /// Quem emite o JWT
        /// Ex: Esta aplicação
        /// </summary>
        public string Emissor { get; set; }

        /// <summary>
        /// Em quais URLs este JWT é válido
        /// </summary>
        public string ValidoEm { get; set; }
    }
}