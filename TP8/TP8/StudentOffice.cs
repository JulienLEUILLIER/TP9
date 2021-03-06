﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TP8
{
    public class StudentOffice : IStudentOffice, ISubscriber
    {

        private IStockBehaviour _stock;

        public IStockBehaviour Stock
        {
            get { return _stock; }
            private set { _stock = value; }
        }


        private Dictionary<Client, decimal> _clientlist;


        public Dictionary<Client, decimal> ClientList 
        {
            get { return _clientlist; }
            private set { _clientlist = value; } 
        }

        private List<Transaction> _transactionsList;
        public List<Transaction> TransactionsList {
            get { return _transactionsList; }
            private set { _transactionsList = value; }
        }


        public readonly Commercial _commercial;


        public StudentOffice(IStockBehaviour stock)
        {
            _clientlist = new Dictionary<Client, decimal>();
            _transactionsList = new List<Transaction>();
            _stock = stock;
            _commercial = new Commercial();
        }


        public IMemento SaveState()
        {
            return new StateOfStudentOffice(
                new Dictionary<Client, decimal>(_clientlist), 
                (IStockData)_stock, 
                new List<Transaction>(_transactionsList));
        }

        public void RestoreState(IMemento memento)
        {
            this.ClientList = memento.GetClientsSnapshot();
            this._stock = (IStockBehaviour)memento.GetStockSnapshot();
            this.TransactionsList = memento.GetTransactionsSnapshot();
        }

        public StudentOffice() 
        {
            ClientList = new Dictionary<Client, decimal>();
        }

        public Client CreateClient(string lastname, string firstname, short age, short year = 0)
        {
            return (year == 0) switch
            {
                true => new OtherClient(lastname, firstname, age),
                false => new Student(lastname, firstname, age, year),
            };
        }

        public bool GetClientByName(string name)
        {
            return ClientList.Any(KeyValuePair => KeyValuePair.Key.GetName().ToUpper().Equals(name.ToUpper()));
        }

        public void AddClient(Client client, decimal balance)
        {
            if (!GetClientByName(client.GetName()))
            {
                ClientList.Add(client, balance);
            }
        }
        public void Update(Product product)
        {
            Order newOrder = _commercial.OrderedProduct(product._name, 40);
            _stock.AddToStock(newOrder);
        }


        public void SellProduct(Client client, Order order)
        {
            if (order._quantity > 0 && client.CanBuy(order._product))
            {
                decimal appropriatePrice = client.GetAppropriatePrice(order._product) * order._quantity;
                ClientList[client] -= appropriatePrice;
                _stock.SellingOperations(appropriatePrice, order);
            }
            TransactionsOperations(client, order);
        }

        private void TransactionsOperations(Client client, Order order)
        {
            if (TransactionsList == null)
            {
                TransactionsList = new List<Transaction>();
            }
            TransactionsList.Add(new Transaction(order._product, order._quantity, client));

        }

        public IVisitor CreateVisitorClient()
        {
            IVisitor visitorClient = new BestClientVisitor();
            AttachTransactionsToVisitor(visitorClient);
            return visitorClient;
        }

        public IVisitor CreateVisitorProduct()
        {
            IVisitor visitorProduct = new BestProductVisitor();
            AttachTransactionsToVisitor(visitorProduct);
            return visitorProduct;
        }

        private void AttachTransactionsToVisitor(IVisitor visitor)
        {
            foreach (var transaction in TransactionsList)
            {
                transaction.Accept(visitor);
            }
        }

        public void SellMealPlan(IAssembler assembler, Client client)
        {
            MealPlan mealPlan = assembler.GetMealPlan();
            foreach (var product in mealPlan.MealProducts)
            {
                SellProduct(client, new Order(product, 1));
            }
        }

        public List<Client> WrongBalance(decimal minimalBalance)
        {
            return ClientList.Keys.Where(kvp => ClientList[kvp] < minimalBalance).ToList();
        }

        public List<Client> WrongBalance()
        {
            return WrongBalance(0m);
        }

        public void DisplayUsersConsole(Dictionary<Client, decimal> list)
        {
            Console.WriteLine("Liste des étudiants de la BDE ayant accès au prix préférentiel :\n");

            foreach (KeyValuePair<Client, decimal> pair in list)
            {
                Console.WriteLine("\t{0} : {1}", pair.Key.GetName(), pair.Value);
            }
            Console.ReadLine();
        }

        public void DisplayWrongBalanceClients(decimal number)
        {
            Console.WriteLine("Liste des mauvais payeurs :\n");

            foreach (Client client in WrongBalance(number))
            {
                Console.WriteLine(client.GetName());
            }
            Console.WriteLine();
        }
    }
}
