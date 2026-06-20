using Domain.Entity;

namespace Application.DTOs;

public static class EncryptionTypeDTOs
{
    public static class Inner
    {
        public record ReadAll();
        public record ReadById(
            int Id
        );
    }
    public static class Request
    {

    }
    public static class Response
    {
        public record Read(
            int Id,
            string Name
        )
        {
            public static Read FromEncryptionType(EncryptionType encryptionType) => new(
                    Id: encryptionType.Id,
                    Name: encryptionType.Name
                );
        }
    }
}