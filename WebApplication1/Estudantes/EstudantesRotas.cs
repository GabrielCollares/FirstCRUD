using ApiCrud.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCrud.Estudantes;

public static class EstudantesRotas {

    public static void AddRotasEstudantes( this WebApplication app) {

        var rotasEstudantes = app.MapGroup("estudantes");   

        rotasEstudantes.MapPost(pattern:"", handler:async (AddEstudanteRequest request, AppDbContext context, CancellationToken ct) => 
        {

            var Existente = await context.Estudantes.AnyAsync(estudante => estudante.Nome == request.Nome); 
            if(Existente) return Results.Conflict("Ja Existente");  
            var novoEstudante = new Estudante(request.Nome);
            await context.Estudantes.AddAsync(novoEstudante, ct);
            await context.SaveChangesAsync(ct);
            var EstudantesRetorno = new EstudantesDto(novoEstudante.Id, novoEstudante.Nome);
        return Results.Ok(novoEstudante);

        });
        rotasEstudantes.MapGet(pattern:"", async(AppDbContext context, CancellationToken ct) => 

        {
            var estudantes = await context
            .Estudantes
            .Where(estudante => estudante.Ativo)
            .Select(estudante => new EstudantesDto(estudante.Id, estudante.Nome))
            .ToListAsync(ct);
            return estudantes;

        });
        rotasEstudantes.MapPut(pattern: "{id:guid}",async (Guid id, UpdateEstudanteRequest request, AppDbContext context, CancellationToken ct) =>
         {
             var estudante = await context.Estudantes
             .SingleOrDefaultAsync(estudante => estudante.Id == id, ct);
             if (estudante == null) return Results.NotFound();
             estudante.AtualizarNome(request.Nome);
             await context.SaveChangesAsync(ct);
             return Results.Ok(new EstudantesDto(estudante.Id, estudante.Nome));
         });
        rotasEstudantes.MapDelete("{id}", async(Guid id, AppDbContext context, CancellationToken ct) => 
        {
            var estudante = await context.Estudantes
            .SingleOrDefaultAsync(estudante => estudante.Id == id, ct);

            if (estudante == null) Results.NotFound();
            estudante.Desativar();
            await context.SaveChangesAsync();
            return Results.Ok();    
        });
    }
}