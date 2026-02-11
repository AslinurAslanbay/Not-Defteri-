using System.Collections.Generic;
using NotDefteriMvc.Models;

namespace NotDefteriMvc.Services
{
    // Bu interface, notlar üzerinde yapılabilecek temel işlemleri tanımlar.
    // Controller, sadece bu arayüzü görür; JSON'un nasıl işlendiğini bilmek zorunda değildir.
    public interface INoteRepository
    {
        List<Note> GetAll();
        Note? GetById(int id);
        void Add(Note note);
        void Update(Note note);
        void DeleteMany(IEnumerable<int> ids);
    }
}

