using System.Runtime.InteropServices.Java;
using Domain.Entity;
using Domain.Interfaces.Repositories;

namespace Application.DTOs;

public static class FragmentDTOs
{
    public static class Inner
    {
        public record Create(
            int DocumentId,
            string MarkerName,
            string Value
        )
        {
            public Fragment GetFragment() => new(
                    documentId: DocumentId,
                    markerName: MarkerName,
                    value: Value
                );
        }
        public record Delete(
            int Id
        );
        public record ReadById(
            int Id
        );
        public record ReadByDocumentId(
            int DocumentId
        );
    }
    public static class Request
    {
        
    }
    public static class Response
    {
        public record Create(
            int Id,
            int DocumentId,
            string MarkerName,
            string Value
        )
        {
            public static Create FromFragment(Fragment fragment) => new(
                    Id: fragment.Id,
                    DocumentId: fragment.DocumentId,
                    MarkerName: fragment.MarkerName,
                    Value: fragment.Value
                );
        }
        public record Read(
            int Id,
            int DocumentId,
            string MarkerName,
            string Value
        )
        {
            public static Read FromFragment(Fragment fragment) => new(
                    Id: fragment.Id,
                    DocumentId: fragment.DocumentId,
                    MarkerName: fragment.MarkerName,
                    Value: fragment.Value
                );
        }
    }
}