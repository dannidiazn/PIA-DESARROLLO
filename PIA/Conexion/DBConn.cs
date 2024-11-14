using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PIA.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Firebase.Storage;


namespace PIA.Conexion
{
    public class DBConn
    {
        public const string WepApyAuthentication = "AIzaSyBoaoprmEY0kBqX_4XuriTvyytLAxReJw0";
        private static readonly HttpClient client = new HttpClient();
        private readonly FirebaseStorage firebaseStorage;

        public DBConn()
        {
            firebaseStorage = new FirebaseStorage("sazonsocial-edbcd.firebasestorage.app");
        }

        public async Task SaveUserProfileImageAsync(string userId, string imageUrl, string idToken)
        {
            var url = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios/{userId}/ProfileImageUrl.json?auth={idToken}";
            var content = JsonConvert.SerializeObject(imageUrl); 
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, httpContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error al guardar la URL de la imagen de perfil en la base de datos: " + await response.Content.ReadAsStringAsync());
            }
        }

        public async Task SaveRecipeImageAsync(string userId, string recipeId, string imageUrl, string idToken)
        {
            var url = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios/{userId}/recetas/{recipeId}/ImageUrl.json?auth={idToken}";
            var content = new { ImageUrl = imageUrl };
            var json = JsonConvert.SerializeObject(content);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, httpContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error al guardar la URL de la imagen de la receta en la base de datos: " + await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string folderName = "recetas")
        {
            try
            {
                var imageRef = firebaseStorage
                    .Child(folderName)
                    .Child(fileName);

                await imageRef.PutAsync(imageStream);
                var downloadUrl = await imageRef.GetDownloadUrlAsync();
                return downloadUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al subir la imagen: {ex.Message}");
                throw;
            }
        }


        public async Task SaveUserProfileImageInDatabaseAsync(string userId, string imageUrl, string idToken)
        {
            var url = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios/{userId}/ProfileImageUrl.json?auth={idToken}";
            var content = new { ProfileImageUrl = imageUrl };
            var json = JsonConvert.SerializeObject(content);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, httpContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error al guardar la URL de la imagen de perfil en la base de datos: " + await response.Content.ReadAsStringAsync());
            }
        }



        public async Task<string> LoginUserAsync(string email, string password)
        {
            var loginUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={WepApyAuthentication}";

            var content = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            var json = JsonConvert.SerializeObject(content);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(loginUrl, httpContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                await SecureStorage.SetAsync("idToken", result.idToken);
                Console.WriteLine($"idToken guardado en SecureStorage: {result.idToken}");

                return result.idToken;
            }
            else
            {
                throw new Exception("Error en el inicio de sesión: " + responseContent);
            }
        }


        public async Task SendPasswordResetEmailAsync(string email)
        {
            var resetUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={WepApyAuthentication}";

            var content = new
            {
                requestType = "PASSWORD_RESET",
                email = email
            };

            var json = JsonConvert.SerializeObject(content);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(resetUrl, httpContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error al enviar el correo de recuperación: " + responseContent);
            }
        }

        public async Task<string> RegisterUserAsync(string email, string password)
        {
            var registerUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={WepApyAuthentication}";

            var content = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            var json = JsonConvert.SerializeObject(content);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(registerUrl, httpContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<RegisterResponse>(responseContent);
                return result.localId; 
            }
            else
            {
                throw new Exception("Error en el registro: " + responseContent);
            }
        }

        public async Task<string> GetUserIdByEmailAsync(string email)
        {
            var url = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios.json";
            var response = await client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var users = JsonConvert.DeserializeObject<Dictionary<string, User>>(responseContent);
                foreach (var user in users)
                {
                    if (user.Value.EmailField == email)
                    {
                        return user.Key; 
                    }
                }
            }

            return null; 
        }

        public async Task SaveUserDataAsync(string userId, object userData, string idToken)
        {
            var saveUrl = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios/{userId}.json?auth={idToken}";

            var json = JsonConvert.SerializeObject(userData);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(saveUrl, httpContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error al guardar los datos del usuario: " + await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<User> GetUserDataAsync(string userId, string idToken)
        {
            if (string.IsNullOrEmpty(idToken))
            {
                Console.WriteLine("El idToken está vacío. Asegúrate de que esté guardado y se esté pasando correctamente.");
                throw new Exception("El idToken está vacío o nulo.");
            }

            var url = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios/{userId}.json?auth={idToken}";
            var response = await client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Respuesta de Firebase: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var jsonObject = JObject.Parse(responseContent);

                    var user = new User
                    {
                        NombreField = jsonObject["NombreField"]?.ToString(),
                        ApellidoField = jsonObject["ApellidoField"]?.ToString(),
                        Edad = jsonObject["Edad"]?.ToObject<int>() ?? 0,
                        EmailField = jsonObject["EmailField"]?.ToString(),
                        TelefonoField = jsonObject["TelefonoField"]?.ToString(),
                        ProfileImageUrl = jsonObject["ProfileImageUrl"]?.ToString()
                    };

                    return user;
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error de deserialización: {ex.Message}");
                    throw new Exception("Error al deserializar los datos del usuario. Verifica que el JSON tenga el formato correcto.");
                }
            }
            else
            {
                Console.WriteLine("Error al obtener los datos del usuario.");
                throw new Exception("Error al obtener los datos del usuario desde Firebase.");
            }
        }


        public async Task SaveRecipeAsync(Receta recipe)
        {
            try
            {
                if (recipe.Ingredientes == null)
                {
                    recipe.Ingredientes = new List<string>(); 
                }

                var saveUrl = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/recetas/{recipe.Id}.json";
                var json = JsonConvert.SerializeObject(recipe); 

                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(saveUrl, httpContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error al guardar la receta: " + await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en SaveRecipeAsync: {ex.Message}");
                throw;
            }
        }


        public async Task SaveUserRecipeAsync(string userId, Receta recipe)
        {
            try
            {
                var recipeId = recipe.Id;

                var saveUrl = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios/{userId}/recetas/{recipeId}.json";

                var jsonObject = new JObject
                {
                    ["Id"] = recipeId,
                    ["Nombre"] = recipe.Nombre,
                    ["Instrucciones"] = recipe.Instrucciones,
                    ["Publica"] = recipe.Publica,
                    ["Ingredientes"] = JArray.FromObject(recipe.Ingredientes),
                    ["ImageUrl"] = recipe.ImageUrl 
                };

                var json = jsonObject.ToString();
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(saveUrl, httpContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error al guardar la receta: " + await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en SaveUserRecipeAsync: {ex.Message}");
                throw;
            }
        }


        public async Task<List<Receta>> GetUserRecipesAsync(string userId)
        {
            var url = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios/{userId}/recetas.json";
            var response = await client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var recipesDictionary = JsonConvert.DeserializeObject<Dictionary<string, Receta>>(responseContent);
                return recipesDictionary?.Values.ToList() ?? new List<Receta>();
            }
            else
            {
                throw new Exception("Error al obtener las recetas del usuario: " + responseContent);
            }
        }


        public async Task<List<Receta>> GetRecipesAsync()
        {
            var getUrl = "https://sazonsocial-edbcd-default-rtdb.firebaseio.com/recetas.json";
            var response = await client.GetAsync(getUrl);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var recipesDictionary = JsonConvert.DeserializeObject<Dictionary<string, Receta>>(responseContent);
                if (recipesDictionary != null)
                {
                    return recipesDictionary.Values.ToList();
                }
            }
            else
            {
                throw new Exception("Error al obtener las recetas: " + responseContent);
            }

            return new List<Receta>();
        }

        public async Task UpdateUserRecipeAsync(string userId, Receta recipe)
        {
            try
            {
                var updateUrl = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios/{userId}/recetas/{recipe.Id}.json";
                var json = JsonConvert.SerializeObject(recipe);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(updateUrl, httpContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error al actualizar la receta: " + await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en UpdateUserRecipeAsync: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteUserRecipeAsync(string userId, string recipeId)
        {
            try
            {
                var deleteUrl = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios/{userId}/recetas/{recipeId}.json";

                var response = await client.DeleteAsync(deleteUrl);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error al eliminar la receta: " + await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en DeleteUserRecipeAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Receta>> GetPublicRecipesAsync()
        {
            var getUrl = "https://sazonsocial-edbcd-default-rtdb.firebaseio.com/recetas.json";
            var response = await client.GetAsync(getUrl);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var recipesDictionary = JsonConvert.DeserializeObject<Dictionary<string, Receta>>(responseContent);
                return recipesDictionary?.Values.ToList() ?? new List<Receta>();
            }
            else
            {
                Console.WriteLine("Error al obtener las recetas públicas: " + responseContent);
                return new List<Receta>();
            }
        }

        public async Task<List<Receta>> GetAllUsersRecipesAsync()
        {
            var url = "https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios.json";
            var response = await client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var usersDictionary = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(responseContent);
                var publicRecipes = new List<Receta>();

                foreach (var user in usersDictionary)
                {
                    if (user.Value.TryGetValue("recetas", out JToken recetasToken) && recetasToken is JObject recetasObject)
                    {
                        var recetasDictionary = recetasObject.ToObject<Dictionary<string, Receta>>();
                        publicRecipes.AddRange(recetasDictionary.Values.Where(r => r.Publica));
                    }
                }
                return publicRecipes;
            }
            throw new Exception("Error al obtener las recetas públicas.");
        }

        public async Task<List<Receta>> GetUserFavoriteRecipesAsync(string userId)
        {
            var url = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios/{userId}/favoritos.json";
            var response = await client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var favoriteRecipesDictionary = JsonConvert.DeserializeObject<Dictionary<string, Receta>>(responseContent);
                    return favoriteRecipesDictionary?.Values.ToList() ?? new List<Receta>();
                }
                catch (JsonSerializationException ex)
                {
                    Console.WriteLine($"Error al deserializar las recetas favoritas: {ex.Message}");
                }
            }
            throw new Exception("Error al obtener las recetas favoritas.");
        }

        public async Task AddRecipeToFavoritesAsync(string userId, Receta recipe)
        {
            try
            {
               
                var url = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios/{userId}/favoritos/{recipe.Id}.json";

                var json = JsonConvert.SerializeObject(recipe);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(url, httpContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error al agregar la receta a favoritos: " + await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en AddRecipeToFavoritesAsync: {ex.Message}");
                throw;
            }
        }

        public async Task RemoveRecipeFromFavoritesAsync(string userId, string recipeId)
        {
            var url = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios/{userId}/favoritos/{recipeId}.json";
            Console.WriteLine($"URL de eliminación: {url}");

            var response = await client.DeleteAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Respuesta de eliminación: {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error al eliminar la receta de favoritos: " + responseContent);
            }
        }


        public async Task<List<Receta>> GetFavoriteRecipesAsync(string userId)
        {
            var url = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios/{userId}/favoritos.json";
            var response = await client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Respuesta de Firebase: " + responseContent);

            if (response.IsSuccessStatusCode)
            {
                
                try
                {
                    var favoritesDictionary = JsonConvert.DeserializeObject<Dictionary<string, Receta>>(responseContent);

                    if (favoritesDictionary != null)
                    {
                        return favoritesDictionary.Values.Where(r => r.Publica).ToList();
                    }
                }
                catch
                {
                    var singleFavorite = JsonConvert.DeserializeObject<Receta>(responseContent);

                    if (singleFavorite != null && singleFavorite.Publica)
                    {
                        return new List<Receta> { singleFavorite };
                    }
                }
            }

            throw new Exception("Error al obtener las recetas favoritas o no se encontraron recetas públicas.");
        }

        public async Task<Receta> GetRecetaAsync(string recetaId)
        {
            var url = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/recetas/{recetaId}.json";
            var response = await client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var jsonObject = JObject.Parse(responseContent);

                if (jsonObject["Ingredientes"] is JArray ingredientesArray)
                {
                    return jsonObject.ToObject<Receta>();
                }
                else if (jsonObject["Ingredientes"] is JObject ingredientesObject)
                {
                    var ingredientesList = ingredientesObject.Properties()
                        .Select(p => p.Value.ToString())
                        .ToList();

                    jsonObject["Ingredientes"] = JArray.FromObject(ingredientesList);

                    return jsonObject.ToObject<Receta>();
                }
            }

            throw new Exception($"Error al obtener la receta: {responseContent}");
        }


        public async Task DeleteUserAccountAsync(string userId, string idToken)
        {
            try
            {
                var deleteUrl = $"https://sazonsocial-edbcd-default-rtdb.firebaseio.com/usuarios/{userId}.json?auth={idToken}";

                var response = await client.DeleteAsync(deleteUrl);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error al eliminar la cuenta de usuario: " + await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar la cuenta de usuario: " + ex.Message);
            }
        }



    }

    public class LoginResponse
    {
        public string idToken { get; set; }
    }

    public class RegisterResponse
    {
        public string localId { get; set; }
    }
}
