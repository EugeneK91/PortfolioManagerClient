using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ORM;

namespace Proxy
{


    public class MediatorService { 

        private IRepository _repositoryService;
        private OperationRepository _operationRepository;
        private PortfolioItemsService _remoteService;
        private int _userId;

        public MediatorService(int userId)
        {
            _remoteService = new PortfolioItemsService();
            _repositoryService = new Repository();
            _operationRepository = new OperationRepository();
            _userId = userId;
        }

        public void Create(PortfolioItem item)
        {
            item.UserId = _userId;
            PortfolioOperation opItem = new PortfolioOperation(item);
            opItem.Operation = "create";
            opItem = _operationRepository.Create(opItem);            

            try
            {
                _remoteService.CreateItem(item);
            }
            catch { }

            var newItem = _remoteService.GetItems(_userId).FirstOrDefault(c => c.Symbol.Equals(item.Symbol));
            _repositoryService.Create(newItem);

            _operationRepository.Delete(opItem.Id);
        }

        public void Edit(PortfolioItem item)
        {
            PortfolioOperation opItem = new PortfolioOperation(item);
            opItem.Operation = "edit";
            opItem = _operationRepository.Create(opItem);

            item.UserId = _userId;
            try
            {
                _remoteService.UpdateItem(item);
                _repositoryService.Edit(item);
            }
            catch { }

            _operationRepository.Delete(opItem.Id);
        }

        public void Delete(int id)
        {
            var item =_repositoryService.GetAll().Where(c=> c.ItemId == id).FirstOrDefault();
            PortfolioOperation opItem = new PortfolioOperation(item);            
            opItem.Operation = "delete";
            opItem = _operationRepository.Create(opItem);

            try
            {
                _remoteService.DeleteItem(id);
                _repositoryService.Delete(id);
            }
            catch { }

            _operationRepository.Delete(opItem.Id);
        }


        public IEnumerable<PortfolioItem> GetAllLocal()
        {
            return _repositoryService.GetAll();
        }

        public void GetAllRemote()
        {
            var remoteList =  _remoteService.GetItems(_userId);
            var localList = _repositoryService.GetAll();

            var deletedItemList = localList.Except(remoteList,new PortfolioComparer()).ToList();
            var addedItemList = remoteList.Except(localList, new PortfolioComparer()).ToList();

            foreach (var item in deletedItemList)
            { _repositoryService.Delete(item.ItemId.Value); }
            //TODO : Block delete/create

            localList =  localList.Except(deletedItemList);
            foreach (var item in localList)
            {
                var temp = remoteList.FirstOrDefault(c => c.ItemId == item.ItemId);
                if (item.SharesNumber != temp.SharesNumber)
                {
                    _repositoryService.Edit(temp);
                }
            }

            foreach (var item in addedItemList)
            {
                _repositoryService.Create(item);
            }

        }
        public IList<PortfolioOperation> GetOperations()
        {
            return _operationRepository.GetAll().ToList();
        }
    }

    public class PortfolioComparer : IEqualityComparer<PortfolioItem>
    {
        public int GetHashCode(PortfolioItem obj)
        {
            return obj.ItemId.Value;
        }

        bool IEqualityComparer<PortfolioItem>.Equals(PortfolioItem x, PortfolioItem y)
        {
            return x.ItemId == y.ItemId;
        }


    }


}
