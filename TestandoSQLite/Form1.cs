using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestandoSQLite
{
    public partial class Form1 : Form
    {
        private static SqlConnection connection = null;
        private static SQLiteConnection sql_con = new SQLiteConnection("Data Source=" + System.AppDomain.CurrentDomain.BaseDirectory.ToString().Replace("\\bin\\Debug\\", "") + ConfigurationManager.AppSettings["PathCCVBUF"].ToString());
        private DbConnection _connection;
        private SQLiteConnection _connectionSqlite;
        protected static SQLiteConnection SetConnection()
        {
            if (sql_con.State != System.Data.ConnectionState.Open)
                sql_con.Open();
            return sql_con;
        }

        public Form1()
        {
            InitializeComponent();
        }

        [Table("Lancamento")]
        public class Lancamento
        {

            public int id { get; set; }

            /// <summary>
            /// Utilizado para definir se é uma modificação de lancamento - Pode ocorrer em um Lancamento Virtual de Substituição
            /// </summary>
            public String idLancamentoInterno { get; set; }
            public int idLancamentoSinacor { get; set; }

            public DateTime dtLancamento { get; set; }
            public DateTime dtReferencia { get; set; }
            public DateTime? dtLiquidacao { get; set; }
            public char situacao { get; set; }

            public string descricao { get; set; }

            public decimal valor { get; set; }

            public int diasProjetados { get; set; }
            public int idLancamentoExterno { get; set; }

            public bool estorno { get; set; }

            public DateTime? dtOrigem { get; set; }
            public DateTime? dtVencimento { get; set; }


            public bool aguardandoRetornoSinacor { get; set; }

            public bool CalculaSaldo { get; set; }



        }



        protected static SqlConnection GetOpenConnection()
        {
            connection = new SqlConnection("Data Source = DTRJ011; Initial Catalog = BusVirtualFinance; Integrated Security = False; Connect Timeout = 60; MultipleActiveResultSets = True; User Id = usrbvf; Password =#Abc1234");
            connection.Open();
            return connection;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {

            _connection = GetOpenConnection();

            var Return = _connection.GetList<Lancamento>();


            _connectionSqlite = SetConnection();


            DateTime TempoProcessamento = DateTime.Now;
            foreach (Lancamento lancamento in Return.Take(200))
            {
                SQLiteCommand insertSQL = new SQLiteCommand("INSERT INTO[Lancamento] (" +
                           " dtLancamento," +
                           " dtReferencia," +
                           " dtLiquidacao," +
                           " dtVencimento," +
                           " dtOrigem," +
                           " situacao," +
                           " descricao," +
                          "  valor," +
                          "  diasProjetados," +
                          "  idLancamentoExterno," +
                          "  aguardandoRetornoSinacor," +
                          "  idLancamentoSinacor," +
                          " estorno," +
                          " idLancamentoInterno," +
                          " CalculaSaldo) " +
                          " values( " +
                           " @dtLancamento," +
                           " @dtReferencia," +
                           " @dtLiquidacao," +
                           " @dtVencimento," +
                           " @dtOrigem," +
                           " @situacao," +
                           " @descricao," +
                          " @valor," +
                          " @diasProjetados," +
                          "  @idLancamentoExterno," +
                          " @aguardandoRetornoSinacor," +
                          "  @idLancamentoSinacor," +
                          "  @estorno," +
                          " @idLancamentoInterno," +
                          "  @CalculaSaldo); ", _connectionSqlite);

                insertSQL.Parameters.AddWithValue("@dtLancamento", lancamento.dtLancamento);
                insertSQL.Parameters.AddWithValue("@dtReferencia", lancamento.dtReferencia);
                insertSQL.Parameters.AddWithValue("@dtLiquidacao", lancamento.dtLiquidacao);
                insertSQL.Parameters.AddWithValue("@dtVencimento", lancamento.dtVencimento);
                insertSQL.Parameters.AddWithValue("@dtOrigem", lancamento.dtOrigem);
                insertSQL.Parameters.AddWithValue("@situacao", lancamento.situacao.ToString());
                insertSQL.Parameters.AddWithValue("@descricao", lancamento.descricao);
                insertSQL.Parameters.AddWithValue("@valor", lancamento.valor);
                insertSQL.Parameters.AddWithValue("@diasProjetados", lancamento.diasProjetados);
                insertSQL.Parameters.AddWithValue("@idLancamentoExterno", lancamento.idLancamentoExterno);
                insertSQL.Parameters.AddWithValue("@aguardandoRetornoSinacor", lancamento.aguardandoRetornoSinacor);
                insertSQL.Parameters.AddWithValue("@idLancamentoSinacor", lancamento.idLancamentoSinacor);
                insertSQL.Parameters.AddWithValue("@estorno", lancamento.estorno);
                insertSQL.Parameters.AddWithValue("@idLancamentoInterno", lancamento.idLancamentoInterno);
                insertSQL.Parameters.AddWithValue("@CalculaSaldo", lancamento.CalculaSaldo);

                try
                {
                    using (var transaction = _connectionSqlite.BeginTransaction())
                    {

                        insertSQL.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            TimeSpan DifProcessamento = DateTime.Now - TempoProcessamento;
        }
    }
}
