// DESAFIO: Documentos Confidenciais - SOLUÇÃO: Padrão Proxy

using System;
using System.Collections.Generic;
using System.Threading;

namespace DesignPatternChallenge
{
    public class ConfidentialDocument
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int SecurityLevel { get; set; }
        public long SizeInBytes { get; set; }

        public ConfidentialDocument(string id, string title, string content, int securityLevel)
        {
            Id = id; Title = title; Content = content; SecurityLevel = securityLevel;
            SizeInBytes = content.Length * 2;
        }
    }

    public class User
    {
        public string Username { get; set; }
        public int ClearanceLevel { get; set; }
        public User(string username, int clearanceLevel) { Username = username; ClearanceLevel = clearanceLevel; }
    }

    public interface IDocumentService
    {
        ConfidentialDocument ViewDocument(string documentId, User user);
        void EditDocument(string documentId, User user, string newContent);
    }

    public class DocumentRepository
    {
        private Dictionary<string, ConfidentialDocument> _database;

        public DocumentRepository()
        {
            Console.WriteLine("[Repository] Inicializando...");
            Thread.Sleep(1000);
            _database = new Dictionary<string, ConfidentialDocument>
            {
                ["DOC001"] = new ConfidentialDocument("DOC001", "Relatório Financeiro Q4", "Conteúdo confidencial...", 3),
                ["DOC002"] = new ConfidentialDocument("DOC002", "Estratégia 2025", "Planos estratégicos...", 5),
                ["DOC003"] = new ConfidentialDocument("DOC003", "Manual do Funcionário", "Políticas...", 1)
            };
        }

        public ConfidentialDocument GetDocument(string id)
        {
            Console.WriteLine($"[Repository] Carregando {id}...");
            Thread.Sleep(500);
            return _database.ContainsKey(id) ? _database[id] : null;
        }

        public void UpdateDocument(string id, string content)
        {
            Thread.Sleep(300);
            if (_database.ContainsKey(id)) _database[id].Content = content;
        }
    }

    public class DocumentServiceProxy : IDocumentService
    {
        private DocumentRepository _repository;
        private readonly Dictionary<string, ConfidentialDocument> _cache = new();
        private readonly List<string> _auditLog = new();

        private DocumentRepository Repository => _repository ??= new DocumentRepository();

        public ConfidentialDocument ViewDocument(string documentId, User user)
        {
            _auditLog.Add($"[{DateTime.Now:HH:mm:ss}] {user.Username} acessou {documentId}");
            Console.WriteLine($"[Audit] {_auditLog[^1]}");

            if (!_cache.TryGetValue(documentId, out var doc))
            {
                doc = Repository.GetDocument(documentId);
                if (doc != null) _cache[documentId] = doc;
            }
            else Console.WriteLine($"[Cache] {documentId} em cache");

            if (doc == null) { Console.WriteLine("❌ Documento não encontrado"); return null; }
            if (user.ClearanceLevel < doc.SecurityLevel)
            {
                Console.WriteLine($"❌ Acesso negado! Nível {user.ClearanceLevel} < {doc.SecurityLevel}");
                return null;
            }
            Console.WriteLine($"✅ Acesso permitido: {doc.Title}");
            return doc;
        }

        public void EditDocument(string documentId, User user, string newContent)
        {
            var doc = ViewDocument(documentId, user);
            if (doc == null) return;
            Repository.UpdateDocument(documentId, newContent);
            _cache.Remove(documentId);
            Console.WriteLine("✅ Documento atualizado");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Documentos Confidenciais (Proxy Pattern) ===\n");

            IDocumentService service = new DocumentServiceProxy();
            var manager = new User("joao.silva", 5);
            var employee = new User("maria.santos", 2);

            service.ViewDocument("DOC002", manager);
            service.ViewDocument("DOC002", employee);
            service.ViewDocument("DOC002", manager);
            service.ViewDocument("DOC003", employee);

            Console.WriteLine("\n✓ Proxy: lazy loading, cache, auditoria, controle de acesso");
        }
    }
}
