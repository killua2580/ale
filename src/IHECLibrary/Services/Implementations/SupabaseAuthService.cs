using Supabase;
using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Postgrest.Attributes;
using Postgrest.Models;

namespace IHECLibrary.Services.Implementations
{
    // Define our database models to match the schema
    [Table("users")]
    public class DbUserModel : BaseModel
    {
        [PrimaryKey("user_id")]
        public string? UserId { get; set; }
        
        [Column("email")]
        public string? Email { get; set; }
        
        [Column("password_hash")]
        public string? PasswordHash { get; set; }
        
        [Column("first_name")]
        public string? FirstName { get; set; }
        
        [Column("last_name")]
        public string? LastName { get; set; }
        
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }
        
        [Column("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }
    }
    
    [Table("student_profiles")]
    public class DbStudentProfileModel : BaseModel
    {
        [PrimaryKey("student_id")]
        public string? StudentId { get; set; }
        
        [Column("level_of_study")]
        public string? LevelOfStudy { get; set; }
        
        [Column("field_of_study")]
        public string? FieldOfStudy { get; set; }
    }
    
    [Table("admin_profiles")]
    public class DbAdminProfileModel : BaseModel
    {
        [PrimaryKey("admin_id")]
        public string? AdminId { get; set; }
        
        [Column("job_title")]
        public string? JobTitle { get; set; }
        
        [Column("is_approved")]
        public bool IsApproved { get; set; }
        
        [Column("approved_by")]
        public string? ApprovedBy { get; set; }
    }
    
    public class SupabaseAuthService : IAuthService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly INavigationService _navigationService;

        public SupabaseAuthService(Supabase.Client supabaseClient, INavigationService navigationService)
        {
            _supabaseClient = supabaseClient;
            _navigationService = navigationService;
        }        public async Task<AuthResult> SignInAsync(string email, string password)
        {
            try
            {
                Console.WriteLine($"=== SupabaseAuthService.SignInAsync() called with email: {email} ===");
                
                Console.WriteLine("Calling _supabaseClient.Auth.SignIn...");
                var session = await _supabaseClient.Auth.SignIn(email, password);
                Console.WriteLine($"Supabase SignIn result - Session is null: {session == null}");
                
                if (session != null)
                {
                    Console.WriteLine($"Session created - User ID: {session.User?.Id}, Access Token: {(session.AccessToken != null ? "present" : "null")}");
                    
                    Console.WriteLine("Getting user from session...");
                    var user = await GetUserFromSessionAsync(session);
                    Console.WriteLine($"User retrieved - User is null: {user == null}, User ID: {user?.Id}");
                    
                    Console.WriteLine("=== SupabaseAuthService.SignInAsync() returning success ===");
                    return new AuthResult
                    {
                        Success = true,
                        User = user
                    };
                }
                
                Console.WriteLine("=== SupabaseAuthService.SignInAsync() returning failure - no session ===");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Échec de la connexion. Veuillez vérifier vos identifiants."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== SupabaseAuthService.SignInAsync() exception: {ex} ===");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = $"Erreur lors de la connexion: {ex.Message}"
                };
            }
        }

        public async Task<AuthResult> RegisterAsync(UserRegistrationModel model)
        {
            try
            {
                if (model == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = "Les données d'inscription sont invalides."
                    };
                }
                
                // Créer l'utilisateur dans Supabase Auth
                var signUpOptions = new SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        { "first_name", model.FirstName },
                        { "last_name", model.LastName },
                        { "phone_number", model.PhoneNumber },
                        { "level_of_study", model.LevelOfStudy },
                        { "field_of_study", model.FieldOfStudy }
                    }
                };

                var session = await _supabaseClient.Auth.SignUp(model.Email, model.Password, signUpOptions);
                
                if (session != null && session.User != null)
                {
                    // Insert into Users table
                    await _supabaseClient.From<DbUserModel>()
                        .Insert(new DbUserModel
                        {
                            UserId = session.User.Id,
                            Email = model.Email,
                            PasswordHash = "", // Password is handled by Supabase Auth
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            PhoneNumber = model.PhoneNumber,
                            ProfilePictureUrl = model.ProfilePictureUrl ?? ""
                        });

                    // Insert into StudentProfiles table
                    await _supabaseClient.From<DbStudentProfileModel>()
                        .Insert(new DbStudentProfileModel
                        {
                            StudentId = session.User.Id,
                            LevelOfStudy = model.LevelOfStudy,
                            FieldOfStudy = model.FieldOfStudy
                        });

                    var user = await GetUserFromSessionAsync(session);
                    return new AuthResult
                    {
                        Success = true,
                        User = user
                    };
                }
                
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Échec de l'inscription. Veuillez réessayer."
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = $"Erreur lors de l'inscription: {ex.Message}"
                };
            }
        }

        public async Task<AuthResult> RegisterAdminAsync(AdminRegistrationModel model)
        {
            try
            {
                if (model == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = "Les données d'inscription sont invalides."
                    };
                }
                
                // Créer l'utilisateur dans Supabase Auth
                var signUpOptions = new SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        { "first_name", model.FirstName },
                        { "last_name", model.LastName },
                        { "phone_number", model.PhoneNumber },
                        { "job_title", model.JobTitle }
                    }
                };

                var session = await _supabaseClient.Auth.SignUp(model.Email, model.Password, signUpOptions);
                
                if (session != null && session.User != null)
                {
                    // Insert into Users table first
                    await _supabaseClient.From<DbUserModel>()
                        .Insert(new DbUserModel
                        {
                            UserId = session.User.Id,
                            Email = model.Email,
                            PasswordHash = "", // Password is handled by Supabase Auth
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            PhoneNumber = model.PhoneNumber,
                            ProfilePictureUrl = ""
                        });
                        
                    // Then insert into AdminProfiles table
                    await _supabaseClient.From<DbAdminProfileModel>()
                        .Insert(new DbAdminProfileModel
                        {
                            AdminId = session.User.Id,
                            JobTitle = model.JobTitle,
                            IsApproved = false
                        });

                    return new AuthResult
                    {
                        Success = true,
                        Message = "Votre demande d'accès administrateur a été soumise et est en attente d'approbation."
                    };
                }
                
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Échec de l'inscription. Veuillez réessayer."
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = $"Erreur lors de l'inscription: {ex.Message}"
                };
            }
        }

        public Task<AuthResult> SignInWithGoogleAsync()
        {
            try
            {
                // Rediriger vers la page de connexion Google
                var provider = Supabase.Gotrue.Constants.Provider.Google;
                var url = _supabaseClient.Auth.SignIn(provider);
                
                // Ouvrir le navigateur pour la connexion Google
                // Note: Cette partie nécessite une implémentation spécifique pour Avalonia
                // et dépend de la façon dont vous gérez l'authentification OAuth
                
                // Pour l'instant, nous retournons un message d'erreur
                return Task.FromResult(new AuthResult
                {
                    Success = false,
                    ErrorMessage = "La connexion avec Google n'est pas encore implémentée."
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new AuthResult
                {
                    Success = false,
                    ErrorMessage = $"Erreur lors de la connexion avec Google: {ex.Message}"
                });
            }
        }

        public async Task<bool> SignOutAsync()
        {
            try
            {
                await _supabaseClient.Auth.SignOut();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return false;
                }
                
                await _supabaseClient.Auth.ResetPasswordForEmail(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                if (_supabaseClient.Auth.CurrentUser == null || 
                    string.IsNullOrEmpty(_supabaseClient.Auth.CurrentUser.Email) ||
                    string.IsNullOrEmpty(currentPassword) ||
                    string.IsNullOrEmpty(newPassword))
                {
                    return false;
                }
                
                // Vérifier d'abord le mot de passe actuel
                var session = await _supabaseClient.Auth.SignIn(_supabaseClient.Auth.CurrentUser.Email, currentPassword);
                
                if (session != null)
                {
                    // Changer le mot de passe
                    await _supabaseClient.Auth.Update(new UserAttributes { Password = newPassword });
                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool IsAuthenticated()
        {
            try
            {
                var currentUser = _supabaseClient.Auth.CurrentUser;
                var currentSession = _supabaseClient.Auth.CurrentSession;
                
                // First basic check
                var isAuth = currentUser != null && currentSession != null && 
                            !string.IsNullOrEmpty(currentUser.Id) && 
                            !string.IsNullOrEmpty(currentSession.AccessToken);
                            
                Console.WriteLine($"IsAuthenticated check: {isAuth}, CurrentUser ID: {currentUser?.Id ?? "null"}, AccessToken: {(currentSession?.AccessToken != null ? "present" : "null")}");

                // If we have a token, it's good enough - we'll refresh the token when needed
                // This is more forgiving and prevents logout loops
                if (isAuth)
                {
                    return true;
                }
                
                // If there is at least a user but session is missing/invalid, 
                // we'll let the user service try to refresh it
                if (currentUser != null && !string.IsNullOrEmpty(currentUser.Id))
                {
                    Console.WriteLine("User exists but session may be invalid, allowing retry with refresh");
                    return true; 
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in IsAuthenticated: {ex.Message}");
                return false;
            }
        }        private async Task<UserModel?> GetUserFromSessionAsync(Session session)
        {
            try
            {
                Console.WriteLine($"=== GetUserFromSessionAsync() called - Session User ID: {session?.User?.Id} ===");
                
                if (session == null || session.User == null)
                {
                    Console.WriteLine("Session or User is null, returning null");
                    return null;
                }
                
                Console.WriteLine("Querying users table...");
                // Récupérer les informations de base de l'utilisateur
                var user = await _supabaseClient.From<DbUserModel>()
                    .Where(p => p.UserId == session.User.Id)
                    .Single();

                Console.WriteLine($"User query result - User is null: {user == null}");

                if (user == null)
                {
                    Console.WriteLine("User not found in database, creating basic user model");
                    return new UserModel
                    {
                        Id = session.User.Id ?? "",
                        Email = session.User.Email ?? "",
                        IsAdmin = false
                    };
                }

                Console.WriteLine($"User found - Email: {user.Email}, FirstName: {user.FirstName}, LastName: {user.LastName}");

                Console.WriteLine("Querying student profile...");
                // Vérifier si l'utilisateur est un étudiant (peut être null)
                var studentProfile = await _supabaseClient.From<DbStudentProfileModel>()
                    .Where(p => p.StudentId == session.User.Id)
                    .Single();

                Console.WriteLine($"Student profile result - Profile is null: {studentProfile == null}");

                Console.WriteLine("Querying admin profile...");
                // Vérifier si l'utilisateur est un administrateur (peut être null)
                var adminProfile = await _supabaseClient.From<DbAdminProfileModel>()
                    .Where(p => p.AdminId == session.User.Id)
                    .Single();

                Console.WriteLine($"Admin profile result - Profile is null: {adminProfile == null}, IsApproved: {adminProfile?.IsApproved}");

                // Créer le modèle utilisateur avec les informations disponibles
                var userModel = new UserModel
                {
                    Id = user.UserId ?? "",
                    Email = user.Email ?? "",
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    IsAdmin = adminProfile != null && adminProfile.IsApproved
                };

                // Ajouter les informations spécifiques aux étudiants si disponibles
                if (studentProfile != null)
                {
                    userModel.LevelOfStudy = studentProfile.LevelOfStudy;
                    userModel.FieldOfStudy = studentProfile.FieldOfStudy;
                }

                // Si c'est un admin, ajouter les informations spécifiques
                if (adminProfile != null)
                {
                    userModel.JobTitle = adminProfile.JobTitle;
                    userModel.IsApproved = adminProfile.IsApproved;
                }

                Console.WriteLine($"=== GetUserFromSessionAsync() returning user - ID: {userModel.Id}, Email: {userModel.Email}, IsAdmin: {userModel.IsAdmin} ===");
                return userModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== GetUserFromSessionAsync() exception: {ex} ===");
                // Créer un modèle utilisateur de base en cas d'erreur
                if (session?.User != null)
                {
                    Console.WriteLine("Creating fallback user model");
                    return new UserModel
                    {
                        Id = session.User.Id ?? "",
                        Email = session.User.Email ?? "",
                        IsAdmin = false
                    };
                }
                Console.WriteLine("Returning null due to exception");
                return null;
            }
        }
    }
}
