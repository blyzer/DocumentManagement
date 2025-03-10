﻿using DocumentManagement.Application.Mapper;
using DocumentManagement.Domain.Entities;
using DocumentManagement.Persistence;
using DT.Core.Data;
using DT.Core.Data.Models;
using DT.Core.Data.Paged;
using DT.Core.Text;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DocumentManagement.Application.DocumentTypes.Queries
{
    public class SearchDocumentTypesByTokenPagedQueryHandler : IRequestHandler<SearchDocumentTypesByTokenPagedQuery, DataSourceResult>
    {
        private readonly DocumentDbContext _context;
        public SearchDocumentTypesByTokenPagedQueryHandler(DocumentDbContext context)
        {
            _context = context;
        }
        public async Task<DataSourceResult> Handle(SearchDocumentTypesByTokenPagedQuery request, CancellationToken cancellationToken)
        {
            IQueryable<DocumentType> query = _context.DocumentTypes.AsQueryable();
            query = query.Where(c => !c.Deleted);

            if (!string.IsNullOrWhiteSpace(request.Token))
            {
                query = query.Where(c => c.Code.Contains(request.Token)
                || c.Name.Contains(request.Token));
            }

            if (!request.DataSourceRequest.SortDataField.IsNullOrEmpty())
            {
                if (QueryHelper.PropertyExists<DocumentType>(request.DataSourceRequest.SortDataField))
                {
                    switch (request.DataSourceRequest.SortOrder)
                    {
                        case "asc":
                            query = QueryHelper.OrderByProperty(query, request.DataSourceRequest.SortDataField);
                            break;
                        case "desc":
                            query = QueryHelper.OrderByPropertyDescending(query, request.DataSourceRequest.SortDataField);
                            break;
                        default:
                            query = query.OrderByDescending(u => u.CreatedOn);
                            break;
                    }
                }
            }
            else
            {
                query = query.OrderByDescending(u => u.CreatedOn);
            }

            PagedList<DocumentType> queryResult = new PagedList<DocumentType>();
            await queryResult.CreateAsync(query, request.DataSourceRequest.PageNum, request.DataSourceRequest.PageSize);
            List<SearchDocumentTypesByTokenPagedDto> data = queryResult.Select(u => u.ToSearchDocumentTypesByTokenPagedDto()).ToList();

            return new DataSourceResult
            {
                Data = data,
                Total = queryResult.TotalCount
            };
        }
    }
}
