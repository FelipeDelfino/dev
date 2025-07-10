using System.Globalization;

namespace Questao1
{
    class ContaBancaria {
        public int Numero { get; }
        public string Titular { get; private set; }
        public double Saldo { get; private set; }

        private const double TaxaSaque = 3.50;

        public ContaBancaria(int numero, string titular){

            Numero = numero;
            Titular = titular;
            Saldo = 0.0;
        }

        public ContaBancaria(int numero, string titular, double depositoInicial) : this(numero, titular){

            Deposito(depositoInicial);
        }

        public void Deposito(double valor){

            Saldo += valor;
        }

        public void Saque(double valor){

            Saldo -= valor + TaxaSaque;
        }

        public void AlterarTitular(string novoTitular){

            Titular = novoTitular;
        }

        public override string ToString(){

            return $"Conta {Numero}, Titular: {Titular}, Saldo: $ {Saldo:F2}";
        }

    }
}
