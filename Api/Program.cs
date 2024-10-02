using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimals_api.Dominio.DTOs;
using minimals_api.Dominio.Entitdades;
using minimals_api.Dominio.Enums;
using minimals_api.Dominio.Interfaces;
using minimals_api.Dominio.ModelViews;
using minimals_api.Dominio.Servicos;
using minimals_api.Infraestrutura.Db;

#region Builder

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRouting(option => option.LowercaseUrls = true);

        var key = builder.Configuration["Jwt:Key"]!.ToString();
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(option =>
        {
            option.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

        builder.Services.AddAuthorization();

        builder.Services.AddScoped<IAdministradorService, AdministradorService>();
        builder.Services.AddScoped<IVeiculoService, VeiculoService>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT aqui"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
            });
        });

        var connectionString = builder.Configuration.GetConnectionString("MySql");
        builder.Services.AddDbContext<DbContexto>(options =>
        {
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            );
        });

        var app = builder.Build();

        #endregion

        #region Home

        app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");

        #endregion

        #region Administradores

        string GerarTokenJwt(Administrador adm)
        {
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>() {
        new Claim("Email", adm.Email),
        new Claim("Perfil", adm.Perfil),
        new Claim(ClaimTypes.Role, adm.Perfil),
    };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorService service) =>
        {
            var adm = service.Login(loginDTO);

            if (adm != null)
            {
                var token = GerarTokenJwt(adm);

                return Results.Ok(new AdmLogadoModelView
                {
                    Email = adm.Email,
                    Perfil = adm.Perfil,
                    Token = token
                });
            }
            else
                return Results.Unauthorized();
        }).AllowAnonymous().WithTags("Administradores");

        app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorService service) =>
        {
            var validacao = new ErrosDeValidacao();

            if (string.IsNullOrWhiteSpace(administradorDTO.Email))
                validacao.Mensagens.Add("Email não pode ser vazio");
            if (string.IsNullOrWhiteSpace(administradorDTO.Senha))
                validacao.Mensagens.Add("Senha não pode ser vazio");
            if (administradorDTO.Perfil == null)
                validacao.Mensagens.Add("Perfil não pode ser vazio");

            if (validacao?.Mensagens?.Count > 0)
                return Results.BadRequest(validacao);

            var adm = new Administrador
            {
                Email = administradorDTO.Email,
                Senha = administradorDTO.Senha,
                Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
            };

            service.Incluir(adm);

            return Results.Created($"/administradores/{adm.Id}", new AdministradorModelView
            {
                Id = adm.Id,
                Email = adm.Email,
                Perfil = adm.Perfil
            });
        }).RequireAuthorization(new AuthorizeAttribute { Roles = nameof(Perfil.Adm) }).WithTags("Administradores");

        app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorService service) =>
        {
            var admsModelView = new List<AdministradorModelView>();
            var adms = service.Todos(pagina);

            foreach (var adm in adms)
            {
                admsModelView.Add(new AdministradorModelView
                {
                    Id = adm.Id,
                    Email = adm.Email,
                    Perfil = adm.Perfil
                });
            }

            return Results.Ok(service.Todos(pagina));
        }).RequireAuthorization(new AuthorizeAttribute { Roles = nameof(Perfil.Adm) }).WithTags("Administradores");

        app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorService service) =>
        {
            var adm = service.BuscarPorId(id);

            if (adm is null)
                return Results.NotFound();

            return Results.Ok(new AdministradorModelView
            {
                Id = adm.Id,
                Email = adm.Email,
                Perfil = adm.Perfil
            });
        }).RequireAuthorization(new AuthorizeAttribute { Roles = nameof(Perfil.Adm) }).WithTags("Administradores");

        #endregion

        #region Veiculos

        ErrosDeValidacao ValidaDto(VeiculoDTO veiculoDTO)
        {
            var validacao = new ErrosDeValidacao();

            if (string.IsNullOrWhiteSpace(veiculoDTO.Nome))
                validacao.Mensagens.Add("O nome não pode ser vazio");
            if (string.IsNullOrWhiteSpace(veiculoDTO.Marca))
                validacao.Mensagens.Add("O marca não pode ser vazio");
            if (veiculoDTO.Ano < 1950)
                validacao.Mensagens.Add("Veiculo muito antigo, aceito somente anos superiores a 1950");

            return validacao;
        }

        app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoService service) =>
        {
            var validacao = ValidaDto(veiculoDTO);
            if (validacao?.Mensagens?.Count > 0)
                return Results.BadRequest(validacao);

            var veiculo = new Veiculo
            {
                Nome = veiculoDTO.Nome,
                Marca = veiculoDTO.Marca,
                Ano = veiculoDTO.Ano
            };

            service.Incluir(veiculo);

            return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = $"{nameof(Perfil.Adm)}, {nameof(Perfil.Editor)}" }).WithTags("Veiculos");

        app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoService service) =>
        {
            var veiculos = service.Todos(pagina);

            return Results.Ok(veiculos);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = $"{nameof(Perfil.Adm)}, {nameof(Perfil.Editor)}" }).WithTags("Veiculos");

        app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoService service) =>
        {
            var veiculo = service.BuscarPorId(id);

            if (veiculo is null)
                return Results.NotFound();

            return Results.Ok(veiculo);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = $"{nameof(Perfil.Adm)}, {nameof(Perfil.Editor)}" }).WithTags("Veiculos");

        app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoService service) =>
        {
            var veiculo = service.BuscarPorId(id);

            if (veiculo is null)
                return Results.NotFound();

            var validacao = ValidaDto(veiculoDTO);
            if (validacao?.Mensagens?.Count > 0)
                return Results.BadRequest(validacao);

            veiculo.Nome = veiculoDTO.Nome;
            veiculo.Marca = veiculoDTO.Marca;
            veiculo.Ano = veiculoDTO.Ano;

            service.Atualizar(veiculo);

            return Results.Ok(veiculo);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = $"{nameof(Perfil.Adm)}" }).WithTags("Veiculos");

        app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoService service) =>
        {
            var veiculo = service.BuscarPorId(id);

            if (veiculo is null)
                return Results.NotFound();

            service.Apagar(veiculo);

            return Results.NoContent();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = $"{nameof(Perfil.Adm)}" }).WithTags("Veiculos");

        #endregion

        #region App

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseAuthentication();
        app.UseAuthorization();

        app.Run();
    }
}

#endregion
#region Home

#endregion
#region Administradores

#endregion
#region Veiculos

#endregion
#region App

#endregion