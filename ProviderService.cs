using Sabio.Data;
using Sabio.Data.Providers;
using Sabio.Models;
using Sabio.Models.Domain;
using Sabio.Models.Domain.Provider;
using Sabio.Models.Requests.ProviderDetails;
using Sabio.Models.Requests.ProviderDetails.Details;
using Sabio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Sabio.Services
{
    public class ProviderService : IProviderService
    {

        IDataProvider _data = null;
        // ILocationMapper _locationMapper = null;
        //,ILocationMapper locationMapper
        public ProviderService(IDataProvider data )
        {
            _data = data;
           // _locationMapper = locationMapper;
        }

        public Paged<ProviderSearchResult> SearchByZipcode(int pageIndex, int pageSize, string query)
        {
            Paged<ProviderSearchResult> pgResult = null;

            List<ProviderSearchResult> result = null;


            
            int totalCount = 0;
            string procName = "[dbo].[Practice_Provider_Location_SearchByZipcode]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                 {
                     col.AddWithValue("@PageIndex", pageIndex);
                     col.AddWithValue("@PageSize", pageSize);
                     col.AddWithValue("@Query", query);
                 },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;

                    ProviderSearchResult searchedResult = new ProviderSearchResult();
                    searchedResult.Practice = new Practice();
                    searchedResult.LocationType = new LocationType();
                    searchedResult.Location = new Location();
                    searchedResult.UserProfile = new UserProfile();

                    searchedResult.Practice.Id = reader.GetSafeInt32(startingIndex++);
                    searchedResult.Practice.Name = reader.GetSafeString(startingIndex++);
                    searchedResult.Practice.Phone = reader.GetSafeString(startingIndex++);
                    searchedResult.Practice.Fax = reader.GetSafeString(startingIndex++);
                    searchedResult.Practice.Email = reader.GetSafeString(startingIndex++);
                    searchedResult.Practice.SiteUrl = reader.GetSafeString(startingIndex++);
                   // searchedResult.Location = _locationMapper.MapALocation(reader, out startingIndex);
                    searchedResult.LocationType.Name = reader.GetSafeString(startingIndex++);
                    searchedResult.Location.LineOne = reader.GetSafeString(startingIndex++);
                    searchedResult.Location.LineTwo = reader.GetSafeString(startingIndex++);
                    searchedResult.Location.City = reader.GetSafeString(startingIndex++);
                    searchedResult.Location.Zip = reader.GetSafeString(startingIndex++);
                    searchedResult.Location.Latitude = reader.GetSafeDouble(startingIndex++);
                    searchedResult.Location.Longitude = reader.GetSafeDouble(startingIndex++);
                    searchedResult.UserProfile.FirstName = reader.GetSafeString(startingIndex++);
                    searchedResult.UserProfile.LastName = reader.GetSafeString(startingIndex++);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(startingIndex++);
                    }

                    if(result == null)
                    {
                        result = new List<ProviderSearchResult>();
                    }
                    result.Add(searchedResult);

                });
            if(result != null)
            {
                pgResult = new Paged<ProviderSearchResult>(result, pageIndex, pageSize,totalCount);
            }
            return pgResult;

        }

        public Provider GetByUserId(int userId)
        {
            ProfessionalDetail professionalDetail = null;
            TitleType titleType = null;
            GenderType genderType = null;
            Provider provider = null;
            UserProfile userProfile = null;
            string procName = "[dbo].[Users_UserProfiles_Providers_Join_Select_ByUserId_V2]";

            _data.ExecuteCmd(procName, inputParamMapper: delegate (SqlParameterCollection parameterCollection)
            {
                parameterCollection.AddWithValue("@UserId", userId);
            },
            singleRecordMapper: delegate (IDataReader reader, short set)
            {
                provider = MapProvider(reader, professionalDetail, titleType, genderType, provider, userProfile);
            },
            returnParameters: null);

            return provider;
        }

        public Paged<ProviderDetail> GetAll(int pageIndex, int pageSize)
        {
            Paged<ProviderDetail> pagedList = null;

            List<ProviderDetail> list = null;
            int totalCount = 0;

            string procName = "[dbo].[Providers_SelectAllV4]";

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection paramsCollection)
                {
                    paramsCollection.AddWithValue("@PageIndex", pageIndex);
                    paramsCollection.AddWithValue("@PageSize", pageSize);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    ProviderDetail providerDetail;
                    int startingIndex;
                    MapProviders(reader, out providerDetail, out startingIndex);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(startingIndex);
                    }

                    if (list == null)
                    {
                        list = new List<ProviderDetail>();
                    }

                    list.Add(providerDetail);
                });

            if (list != null)
            {
                pagedList = new Paged<ProviderDetail>(list, pageIndex, pageSize, totalCount);
            }
            return pagedList;
        }

        public List<ProviderDetail> Get(int id)
        {
            List<ProviderDetail> providerDetailsList = null;

            Dictionary<int, List<Practice>> practicesByProvider = null;
            Dictionary<int, List<Affiliation>> affiliationsByProvider = null;
            Dictionary<int, List<Language>> languagesByProvider = null;
            Dictionary<int, List<License>> licensesByProvider = null;
            Dictionary<int, List<ProviderMedicalService>> servicesByProvider = null;
            Dictionary<int, List<ProviderSpecialization>> specializationsByProvider = null;

            string procName = "[dbo].[Providers_Select_ByIdV6]";

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection paramsCollection)
                {
                    paramsCollection.AddWithValue("@Id", id);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;

                    switch (set)
                    {
                        /*
                         NOTE-- I'm going to implement this case statement to refactor GetAll and Search, but just wanted to get a pull request done to get more comments
                        */
                        case 0:
                            MapProviderDetails(reader, ref providerDetailsList, ref startingIndex);
                            break;

                        case 1:
                            int providerId;
                            MapPracticeDetails(reader, ref practicesByProvider, ref startingIndex);
                            break;

                        case 2:
                            providerId = MapAffiliationDetails(reader, ref affiliationsByProvider, ref startingIndex);
                            break;

                        case 3:
                            providerId = MapLanguageDetails(reader, ref languagesByProvider, ref startingIndex);
                            break;

                        case 4:
                            providerId = MapLicenseDetails(reader, ref licensesByProvider, ref startingIndex);
                            break;

                        case 5:
                            providerId = MapServiceDetails(reader, ref servicesByProvider, ref startingIndex);
                            break;

                        case 6:
                            providerId = MapProviderSpecializations(reader, ref specializationsByProvider, ref startingIndex);
                            break;

                        default:
                            break;
                    }


                });

            if (providerDetailsList != null)
            {
                foreach (ProviderDetail providerDetail in providerDetailsList)
                {
                    if (practicesByProvider.ContainsKey(providerDetail.Id))
                    {
                        providerDetail.Practices = practicesByProvider[providerDetail.Id];
                    }

                    if (affiliationsByProvider.ContainsKey(providerDetail.Id))
                    {
                        providerDetail.Affiliations = affiliationsByProvider[providerDetail.Id];
                    }

                    if (languagesByProvider.ContainsKey(providerDetail.Id))
                    {
                        providerDetail.Languages = languagesByProvider[providerDetail.Id];
                    }

                    if (licensesByProvider.ContainsKey(providerDetail.Id))
                    {
                        providerDetail.Licenses = licensesByProvider[providerDetail.Id];
                    }

                    if (servicesByProvider.ContainsKey(providerDetail.Id))
                    {
                        providerDetail.ProviderServices = servicesByProvider[providerDetail.Id];
                    }

                    if (specializationsByProvider.ContainsKey(providerDetail.Id))
                    {
                        providerDetail.ProviderSpecializations = specializationsByProvider[providerDetail.Id];
                    }

                }
            }
            return providerDetailsList;
        }
       
        public Paged<ProviderDetail> Search(int pageIndex, int pageSize, string Query)
        {
            Paged<ProviderDetail> pagedList = null;
            List<ProviderDetail> list = null;
            int totalCount = 0;

            string procName = "[dbo].[Providers_SearchV2]";

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection paramsCollection)
                {
                    paramsCollection.AddWithValue("@Query", Query);
                    paramsCollection.AddWithValue("@PageIndex", pageIndex);
                    paramsCollection.AddWithValue("@PageSize", pageSize);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    ProviderDetail providerDetail;
                    int startingIndex;
                    MapProviders(reader, out providerDetail, out startingIndex);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(startingIndex);
                    }

                    if (list == null)
                    {
                        list = new List<ProviderDetail>();
                    }

                    list.Add(providerDetail);
                });

            if (list != null)
            {
                pagedList = new Paged<ProviderDetail>(list, pageIndex, pageSize, totalCount);
            }
            return pagedList;
        }

        public int Add(ProviderAddRequest model, int userId)
        {
            int id = 0;
            string procName = "[dbo].[ProviderDetails_Insert_V4]";

            DataTable affiliationsTable = null;
            DataTable certificationsTable = null;
            DataTable expertiseTable = null;
            DataTable languagesTable = null;
            DataTable licensesTable = null;
            DataTable locationsTable = null;
            DataTable practicesTable = null;
            DataTable practiceLanguagesTable = null;
            DataTable servicesTable = null;
            DataTable specializationsTable = null;

            if (model.Affiliations != null)
            {
                affiliationsTable = MapAffiliationsToTable(model.Affiliations);
            }
            if (model.Certifications != null)
            {
                certificationsTable = MapCertificationsToTable(model.Certifications);
            }
            if (model.ExpertiseList != null)
            {
                expertiseTable = MapExpertiseToTable(model.ExpertiseList);
            }
            if (model.Languages != null)
            {
                languagesTable = MapLanguagesToTable(model.Languages);
            }
            if (model.Licenses != null)
            {
                licensesTable = MapLicensesToTable(model.Licenses);
            }
            if (model.Locations != null)
            {
                locationsTable = MapLocationsToTable(model.Locations);
            }
            if (model.Practices != null)
            {
                practicesTable = MapPracticesToTable(model.Practices);
            }
            if (model.PracticeLanguages != null)
            {
                practiceLanguagesTable = MapPracticeLanguagesToTable(model.PracticeLanguages);
            }
            if (model.Services != null)
            {
                servicesTable = MapServicesToTable(model.Services);
            }
            if (model.Specializations != null)
            {
                specializationsTable = MapSpecializationsToTable(model.Specializations);
            }

            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    AddCommonParams(model, col);
                    AddTables(col
                        , affiliationsTable
                        , certificationsTable
                        , expertiseTable
                        , languagesTable
                        , licensesTable
                        , locationsTable
                        , practicesTable
                        , practiceLanguagesTable
                        , servicesTable
                        , specializationsTable
                        );
                    col.AddWithValue("@CreatedBy", userId);

                    SqlParameter idOut = new SqlParameter("@ProviderId", SqlDbType.Int);
                    idOut.Direction = ParameterDirection.Output;

                    col.Add(idOut);
                },
                returnParameters: delegate (SqlParameterCollection returnCollection)
                {
                    object oId = returnCollection["@ProviderId"].Value;
                    int.TryParse(oId.ToString(), out id);
                }
            );
            return id;
        }

      

       
        private static Provider MapProvider(IDataReader reader, ProfessionalDetail professionalDetail, TitleType titleType, GenderType genderType, Provider provider, UserProfile userProfile)
        {
            int startingIndex = 0;
            provider = new Provider();
            provider.Id = reader.GetSafeInt32(startingIndex++);
            provider.Phone = reader.GetSafeString(startingIndex++);
            provider.Fax = reader.GetSafeString(startingIndex++);
            provider.Networks = reader.GetSafeString(startingIndex++);


            professionalDetail = new ProfessionalDetail();
            provider.ProfessionalDetail = professionalDetail;
            professionalDetail.Id = reader.GetSafeInt32(startingIndex++);
            professionalDetail.NPI = reader.GetSafeString(startingIndex++);
            professionalDetail.IsAccepting = reader.GetSafeBool(startingIndex++);

            titleType = new TitleType();
            provider.TitleType = titleType;
            titleType.Id = reader.GetSafeInt32(startingIndex++);
            titleType.Name = reader.GetSafeString(startingIndex++);

            userProfile = new UserProfile();
            provider.UserProfile = userProfile;
            userProfile.Id = reader.GetSafeInt32(startingIndex++);
            userProfile.FirstName = reader.GetSafeString(startingIndex++);
            userProfile.LastName = reader.GetSafeString(startingIndex++);
            userProfile.Mi = reader.GetSafeString(startingIndex++);
            userProfile.AvatarUrl = reader.GetSafeString(startingIndex++);
            userProfile.DateCreated = reader.GetSafeDateTime(startingIndex++);
            userProfile.DateModified = reader.GetSafeDateTime(startingIndex++);

            genderType = new GenderType();
            provider.GenderType = genderType;
            genderType.Id = reader.GetSafeInt32(startingIndex++);
            genderType.Name = reader.GetSafeString(startingIndex++);
            return provider;
        }

        #region Get support methods

  

    
        #endregion

        #region Add support methods

        private static void AddCommonParams(ProviderServiceAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@Price", model.Price);
            col.AddWithValue("@ServiceId", model.ServiceId);
            col.AddWithValue("@ServiceTypeId", model.ServiceTypeId);
        }

        #endregion
    

        #region Get support methods

        private static void MapProviders(IDataReader reader, out ProviderDetail providerDetail, out int startingIndex)
        {
            providerDetail = new ProviderDetail();
            startingIndex = 0;
            startingIndex = ProviderDetailMapper(reader, providerDetail, startingIndex);
            startingIndex = ProfessionalDetailMapper(reader, providerDetail, startingIndex);
            startingIndex = TitleTypeMapper(reader, providerDetail, startingIndex);
            startingIndex = UserProfileMapper(reader, providerDetail, startingIndex);
            startingIndex = GenderTypeMapper(reader, providerDetail, startingIndex);

            string affiliationsString = reader.GetSafeString(startingIndex++);
            if (!string.IsNullOrEmpty(affiliationsString))
            {
                providerDetail.Affiliations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Affiliation>>(affiliationsString);
            }

            string languagesString = reader.GetSafeString(startingIndex++);
            if (!string.IsNullOrEmpty(languagesString))
            {
                providerDetail.Languages = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Language>>(languagesString);
            }

            string licensesString = reader.GetSafeString(startingIndex++);
            if (!string.IsNullOrEmpty(licensesString))
            {
                providerDetail.Licenses = Newtonsoft.Json.JsonConvert.DeserializeObject<List<License>>(licensesString);
            }

            string providerSpecializationsString = reader.GetSafeString(startingIndex++);
            if (!string.IsNullOrEmpty(providerSpecializationsString))
            {
                providerDetail.ProviderSpecializations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProviderSpecialization>>(providerSpecializationsString);
            }
        }

        private static ProviderDetail MapProviderDetails(IDataReader reader)
        {
            ProviderDetail providerDetail = new ProviderDetail();

            int startingIndex = 0;
            startingIndex = ProviderDetailMapper(reader, providerDetail, startingIndex);
            startingIndex = ProfessionalDetailMapper(reader, providerDetail, startingIndex);
            startingIndex = TitleTypeMapper(reader, providerDetail, startingIndex);
            startingIndex = UserProfileMapper(reader, providerDetail, startingIndex);
            startingIndex = GenderTypeMapper(reader, providerDetail, startingIndex);

            return providerDetail;
        }


        private static int ProviderDetailMapper(IDataReader reader, ProviderDetail providerDetail, int startingIndex)
        {
            providerDetail.Id = reader.GetSafeInt32(startingIndex++);
            providerDetail.Phone = reader.GetSafeString(startingIndex++);
            providerDetail.Fax = reader.GetSafeString(startingIndex++);
            providerDetail.Networks = reader.GetSafeString(startingIndex++);
            return startingIndex;
        }

        private static int ProfessionalDetailMapper(IDataReader reader, ProviderDetail providerDetail, int startingIndex)
        {
            providerDetail.ProfessionalDetail = new ProfessionalDetail();
            providerDetail.ProfessionalDetail.Id = reader.GetSafeInt32(startingIndex++);
            providerDetail.ProfessionalDetail.NPI = reader.GetSafeString(startingIndex++);
            providerDetail.ProfessionalDetail.GenderAccepted = new GenderType();
            providerDetail.ProfessionalDetail.GenderAccepted.Id = reader.GetSafeInt32(startingIndex++);
            providerDetail.ProfessionalDetail.GenderAccepted.Name = reader.GetSafeString(startingIndex++);
            providerDetail.ProfessionalDetail.IsAccepting = reader.GetSafeBool(startingIndex++);
            return startingIndex;
        }

        private static int TitleTypeMapper(IDataReader reader, ProviderDetail providerDetail, int startingIndex)
        {
            providerDetail.TitleType = new TitleType();
            providerDetail.TitleType.Id = reader.GetSafeInt32(startingIndex++);
            providerDetail.TitleType.Name = reader.GetSafeString(startingIndex++);
            return startingIndex;
        }

        private static int UserProfileMapper(IDataReader reader, ProviderDetail providerDetail, int startingIndex)
        {
            providerDetail.UserProfile = new UserProfile();
            providerDetail.UserProfile.Id = reader.GetSafeInt32(startingIndex++);
            providerDetail.UserProfile.UserId = reader.GetSafeInt32(startingIndex++);
            providerDetail.UserProfile.FirstName = reader.GetSafeString(startingIndex++);
            providerDetail.UserProfile.LastName = reader.GetSafeString(startingIndex++);
            providerDetail.UserProfile.Mi = reader.GetSafeString(startingIndex++);
            providerDetail.UserProfile.AvatarUrl = reader.GetSafeString(startingIndex++);
            return startingIndex;
        }

        private static int GenderTypeMapper(IDataReader reader, ProviderDetail providerDetail, int startingIndex)
        {
            providerDetail.GenderType = new GenderType();
            providerDetail.GenderType.Id = reader.GetSafeInt32(startingIndex++);
            providerDetail.GenderType.Name = reader.GetSafeString(startingIndex++);
            return startingIndex;
        }

        #endregion

        #region GetById support methods

        private static int MapProviderSpecializations(IDataReader reader, ref Dictionary<int, List<ProviderSpecialization>> specializationsByProvider, ref int startingIndex)
        {
            int providerId;
            ProviderSpecialization specializationDetails = new ProviderSpecialization();
            specializationDetails.IsPrimary = reader.GetSafeBool(startingIndex++);

            specializationDetails.Specialization = new Specialization();
            specializationDetails.Specialization.Id = reader.GetSafeInt32(startingIndex++);
            specializationDetails.Specialization.Name = reader.GetSafeString(startingIndex++);

            providerId = reader.GetSafeInt32(startingIndex++);

            if (specializationsByProvider == null)
            {
                specializationsByProvider = new Dictionary<int, List<ProviderSpecialization>>();
            }

            if (!specializationsByProvider.ContainsKey(providerId))
            {
                specializationsByProvider[providerId] = new List<ProviderSpecialization>();
            }

            specializationsByProvider[providerId].Add(specializationDetails);
            return providerId;
        }

        private static int MapServiceDetails(IDataReader reader, ref Dictionary<int, List<ProviderMedicalService>> servicesByProvider, ref int startingIndex)
        {
            int providerId;
            ProviderMedicalService serviceDetails = new ProviderMedicalService();
            serviceDetails.Id = reader.GetSafeInt32(startingIndex++);
            serviceDetails.Price = reader.GetSafeDecimal(startingIndex++);

            serviceDetails.MedicalService = new MedicalService();
            serviceDetails.MedicalService.Id = reader.GetSafeInt32(startingIndex++);
            serviceDetails.MedicalService.Name = reader.GetSafeString(startingIndex++);
            serviceDetails.MedicalService.Cpt4Code = reader.GetSafeString(startingIndex++);

            serviceDetails.MedicalServiceType = new MedicalServiceType();
            serviceDetails.MedicalServiceType.Id = reader.GetSafeInt32(startingIndex++);
            serviceDetails.MedicalServiceType.Name = reader.GetSafeString(startingIndex++);
            providerId = reader.GetSafeInt32(startingIndex++);

            if (servicesByProvider == null)
            {
                servicesByProvider = new Dictionary<int, List<ProviderMedicalService>>();
            }

            if (!servicesByProvider.ContainsKey(providerId))
            {
                servicesByProvider[providerId] = new List<ProviderMedicalService>();
            }

            servicesByProvider[providerId].Add(serviceDetails);
            return providerId;
        }

        private static int MapLicenseDetails(IDataReader reader, ref Dictionary<int, List<License>> licensesByProvider, ref int startingIndex)
        {
            int providerId;
            License licenseDetails = new License();
            licenseDetails.Id = reader.GetSafeInt32(startingIndex++);

            licenseDetails.State = new State();
            licenseDetails.State.Id = reader.GetSafeInt32(startingIndex++);
            licenseDetails.State.Code = reader.GetSafeString(startingIndex++);
            licenseDetails.State.Name = reader.GetSafeString(startingIndex++);

            licenseDetails.LicenseNumber = reader.GetSafeString(startingIndex++);
            licenseDetails.DateExpires = reader.GetSafeDateTime(startingIndex++);
            providerId = reader.GetSafeInt32(startingIndex++);

            if (licensesByProvider == null)
            {
                licensesByProvider = new Dictionary<int, List<License>>();
            }

            if (!licensesByProvider.ContainsKey(providerId))
            {
                licensesByProvider[providerId] = new List<License>();
            }

            licensesByProvider[providerId].Add(licenseDetails);
            return providerId;
        }

        private static int MapLanguageDetails(IDataReader reader, ref Dictionary<int, List<Language>> languagesByProvider, ref int startingIndex)
        {
            int providerId;
            Language languageDetails = new Language();
            languageDetails.Id = reader.GetSafeInt32(startingIndex++);
            languageDetails.Code = reader.GetSafeString(startingIndex++);
            languageDetails.Name = reader.GetSafeString(startingIndex++);
            providerId = reader.GetSafeInt32(startingIndex++);

            if (languagesByProvider == null)
            {
                languagesByProvider = new Dictionary<int, List<Language>>();
            }

            if (!languagesByProvider.ContainsKey(providerId))
            {
                languagesByProvider[providerId] = new List<Language>();
            }

            languagesByProvider[providerId].Add(languageDetails);
            return providerId;
        }

        private static int MapAffiliationDetails(IDataReader reader, ref Dictionary<int, List<Affiliation>> affiliationsByProvider, ref int startingIndex)
        {
            int providerId;
            Affiliation affiliationDetails = new Affiliation();
            affiliationDetails.Id = reader.GetSafeInt32(startingIndex++);
            affiliationDetails.Name = reader.GetSafeString(startingIndex++);

            affiliationDetails.AffiliationType = new AffiliationType();
            affiliationDetails.AffiliationType.Id = reader.GetSafeInt32(startingIndex++);
            affiliationDetails.AffiliationType.Name = reader.GetSafeString(startingIndex++);
            providerId = reader.GetSafeInt32(startingIndex++);

            if (affiliationsByProvider == null)
            {
                affiliationsByProvider = new Dictionary<int, List<Affiliation>>();
            }

            if (!affiliationsByProvider.ContainsKey(providerId))
            {
                affiliationsByProvider[providerId] = new List<Affiliation>();
            }

            affiliationsByProvider[providerId].Add(affiliationDetails);
            return providerId;
        }

        private static void MapPracticeDetails(IDataReader reader, ref Dictionary<int, List<Practice>> practicesByProvider, ref int startingIndex)
        {
            Practice practiceDetail = new Practice();
            practiceDetail.Id = reader.GetSafeInt32(startingIndex++);
            practiceDetail.Name = reader.GetSafeString(startingIndex++);
            practiceDetail.Phone = reader.GetSafeString(startingIndex++);
            practiceDetail.Fax = reader.GetSafeString(startingIndex++);
            practiceDetail.Email = reader.GetSafeString(startingIndex++);
            practiceDetail.SiteUrl = reader.GetSafeString(startingIndex++);
            practiceDetail.FacilityTypeId = reader.GetSafeInt32(startingIndex++);
            practiceDetail.ScheduleId = reader.GetSafeInt32(startingIndex++);
            practiceDetail.ADAAccessible = reader.GetSafeBool(startingIndex++);
            practiceDetail.InsuranceAccepted = reader.GetSafeBool(startingIndex++);
            practiceDetail.GenderAccepted = reader.GetSafeInt32(startingIndex++);

            practiceDetail.Location = new Location();
            practiceDetail.Location.Id = reader.GetSafeInt32(startingIndex++);

            practiceDetail.Location.LocationType = new LocationType();
            practiceDetail.Location.LocationType.Id = reader.GetSafeInt32(startingIndex++);
            practiceDetail.Location.LocationType.Name = reader.GetSafeString(startingIndex++);
            practiceDetail.Location.LineOne = reader.GetSafeString(startingIndex++);
            practiceDetail.Location.LineTwo = reader.GetSafeString(startingIndex++);
            practiceDetail.Location.City = reader.GetSafeString(startingIndex++);
            practiceDetail.Location.Zip = reader.GetSafeString(startingIndex++);

            practiceDetail.Location.State = new State();
            practiceDetail.Location.State.Id = reader.GetSafeInt32(startingIndex++);
            practiceDetail.Location.State.Code = reader.GetSafeString(startingIndex++);
            practiceDetail.Location.State.Name = reader.GetSafeString(startingIndex++);
            practiceDetail.Location.Latitude = reader.GetSafeDouble(startingIndex++);
            practiceDetail.Location.Longitude = reader.GetSafeDouble(startingIndex++);

            int providerId = reader.GetSafeInt32(startingIndex++);

            if (practicesByProvider == null)
            {
                practicesByProvider = new Dictionary<int, List<Practice>>();
            }

            if (!practicesByProvider.ContainsKey(providerId))
            {
                practicesByProvider[providerId] = new List<Practice>();
            }

            practicesByProvider[providerId].Add(practiceDetail);
        }

        private static void MapProviderDetails(IDataReader reader, ref List<ProviderDetail> providerDetailsList, ref int startingIndex)
        {
            ProviderDetail providerDetail = new ProviderDetail();

            startingIndex = ProviderDetailMapper(reader, providerDetail, startingIndex);
            startingIndex = ProfessionalDetailMapper(reader, providerDetail, startingIndex);
            startingIndex = TitleTypeMapper(reader, providerDetail, startingIndex);
            startingIndex = UserProfileMapper(reader, providerDetail, startingIndex);
            startingIndex = GenderTypeMapper(reader, providerDetail, startingIndex);

            if (providerDetailsList == null)
            {
                providerDetailsList = new List<ProviderDetail>();
            }

            providerDetailsList.Add(providerDetail);
        }

        #endregion

        #region Add support methods
        private static void AddCommonParams(ProviderAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@TitleTypeId", model.TitleTypeId);
            col.AddWithValue("@UserProfileId", model.UserProfileId);
            col.AddWithValue("@GenderTypeId", model.GenderTypeId);
            col.AddWithValue("@Phone", model.Phone);
            col.AddWithValue("@Fax", model.Fax);
            col.AddWithValue("@Networks", model.Networks);
            col.AddWithValue("@NPI", model.NPI);
            col.AddWithValue("@GenderAccepted", model.GenderAccepted);
            col.AddWithValue("@IsAccepting", model.IsAccepting);
        }

        private static void AddTables(SqlParameterCollection col
            , DataTable affiliationTable
            , DataTable certificationsTable
            , DataTable expertiseTable
            , DataTable languagesTable
            , DataTable licensesTable
            , DataTable locationsTable
            , DataTable practicesTable
            , DataTable practiceLanguagesTable
            , DataTable servicesTable
            , DataTable specializationsTable
            )
        {
            col.AddWithValue("@affiliationsToAdd", affiliationTable);
            col.AddWithValue("@certificationsToAdd", certificationsTable);
            col.AddWithValue("@expertiseToAdd", expertiseTable);
            col.AddWithValue("@languagesToAdd", languagesTable);
            col.AddWithValue("@licensesToAdd", licensesTable);
            col.AddWithValue("@locationsToAdd", locationsTable);
            col.AddWithValue("@practicesToAdd", practicesTable);
            col.AddWithValue("@practiceLanguagesToAdd", practiceLanguagesTable);
            col.AddWithValue("@servicesToAdd", servicesTable);
            col.AddWithValue("@specializationsToAdd", specializationsTable);
        }

        private static DataTable MapAffiliationsToTable(List<ProviderAffiliationAddRequest> affiliationsToMap)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("AffiliationTypeId", typeof(Int32));

            foreach (ProviderAffiliationAddRequest item in affiliationsToMap)
            {
                DataRow dr = dt.NewRow();
                int startingIndex = 0;  // column index

                dr.SetField(startingIndex++, item.Name);
                dr.SetField(startingIndex++, item.AffiliationTypeId);

                dt.Rows.Add(dr);
            }
            return dt;
        }

        private static DataTable MapCertificationsToTable(List<ProviderCertificationAddRequest> certificationsToMap)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("[CertificationId]", typeof(Int32));

            foreach (ProviderCertificationAddRequest item in certificationsToMap)
            {
                DataRow dr = dt.NewRow();
                int startingIndex = 0;  // column index

                dr.SetField(startingIndex++, item.CertificationId);

                dt.Rows.Add(dr);
            }
            return dt;
        }

        private static DataTable MapExpertiseToTable(List<ProviderExpertiseAddRequest> expertiseToMap)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Description", typeof(string));

            foreach (ProviderExpertiseAddRequest item in expertiseToMap)
            {
                DataRow dr = dt.NewRow();
                int startingIndex = 0;  // column index

                dr.SetField(startingIndex++, item.Name);
                dr.SetField(startingIndex++, item.Description);

                dt.Rows.Add(dr);
            }
            return dt;
        }

        private static DataTable MapLanguagesToTable(List<ProviderLanguageAddRequest> languagesToMap)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LanguageId", typeof(Int32));

            foreach (ProviderLanguageAddRequest item in languagesToMap)
            {
                DataRow dr = dt.NewRow();
                int startingIndex = 0;  // column index

                dr.SetField(startingIndex++, item.LanguageId);

                dt.Rows.Add(dr);
            }
            return dt;
        }

        private static DataTable MapLicensesToTable(List<ProviderLicenseAddRequest> licensesToMap)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("LicenseStateId", typeof(Int32));
            dt.Columns.Add("LicenseNumber", typeof(string));
            dt.Columns.Add("DateExpires", typeof(DateTime));

            foreach (ProviderLicenseAddRequest item in licensesToMap)
            {
                DataRow dr = dt.NewRow();
                int startingIndex = 0;  // column index

                dr.SetField(startingIndex++, item.Name);
                dr.SetField(startingIndex++, item.LicenseStateId);
                dr.SetField(startingIndex++, item.LicenseNumber);
                dr.SetField(startingIndex++, item.DateExpires);

                dt.Rows.Add(dr);
            }
            return dt;
        }

        private static DataTable MapLocationsToTable(List<PracticeLocationAddRequest> locationsToMap)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("TxId", typeof(Int32));   //TempLocationId
            dt.Columns.Add("LocationTypeId", typeof(Int32));
            dt.Columns.Add("LineOne", typeof(string));
            dt.Columns.Add("LineTwo", typeof(string));
            dt.Columns.Add("City", typeof(string));
            dt.Columns.Add("Zip", typeof(string));
            dt.Columns.Add("StateId", typeof(Int32));
            dt.Columns.Add("Latitude", typeof(double));
            dt.Columns.Add("Longitude", typeof(double));

            foreach (PracticeLocationAddRequest item in locationsToMap)
            {
                DataRow dr = dt.NewRow();
                int startingIndex = 0;  // column index

                dr.SetField(startingIndex++, item.TempLocationId);
                dr.SetField(startingIndex++, item.LocationTypeId);
                dr.SetField(startingIndex++, item.LineOne);
                dr.SetField(startingIndex++, item.LineTwo);
                dr.SetField(startingIndex++, item.City);
                dr.SetField(startingIndex++, item.Zip);
                dr.SetField(startingIndex++, item.StateId);
                dr.SetField(startingIndex++, item.Latitude);
                dr.SetField(startingIndex++, item.Longitude);

                dt.Rows.Add(dr);
            }
            return dt;
        }

        private static DataTable MapPracticesToTable(List<ProviderPracticeAddRequest> practicesToMap)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("TempLocationId", typeof(Int32));
            dt.Columns.Add("TempPracticeId", typeof(Int32));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Phone", typeof(string));
            dt.Columns.Add("Fax", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("SiteUrl", typeof(string));
            dt.Columns.Add("FacilityTypeId", typeof(Int32));
            dt.Columns.Add("ScheduleId", typeof(Int32));
            dt.Columns.Add("AdaAccessible", typeof(bool));
            dt.Columns.Add("InsuranceAccepted", typeof(bool));
            dt.Columns.Add("GenderAccepted", typeof(Int32));

            foreach (ProviderPracticeAddRequest item in practicesToMap)
            {
                DataRow dr = dt.NewRow();
                int startingIndex = 0;  // column index

                dr.SetField(startingIndex++, item.TempLocationId);
                dr.SetField(startingIndex++, item.TempPracticeId);
                dr.SetField(startingIndex++, item.Name);
                dr.SetField(startingIndex++, item.Phone);
                dr.SetField(startingIndex++, item.Fax);
                dr.SetField(startingIndex++, item.Email);
                dr.SetField(startingIndex++, item.SiteUrl);
                dr.SetField(startingIndex++, item.FacilityTypeId);
                dr.SetField(startingIndex++, item.ScheduleId);
                dr.SetField(startingIndex++, item.AdaAccessible);
                dr.SetField(startingIndex++, item.InsuranceAccepted);
                dr.SetField(startingIndex++, item.GenderAccepted);

                dt.Rows.Add(dr);
            }
            return dt;
        }

        private static DataTable MapPracticeLanguagesToTable(List<PracticeLanguageAddRequest> languagesToMap)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("TempPracticeId", typeof(Int32));
            dt.Columns.Add("LanguageId", typeof(Int32));

            foreach (PracticeLanguageAddRequest item in languagesToMap)
            {
                DataRow dr = dt.NewRow();
                int startingIndex = 0;  // column index

                dr.SetField(startingIndex++, item.TempPracticeId);
                dr.SetField(startingIndex++, item.LanguageId);

                dt.Rows.Add(dr);
            }
            return dt;
        }

        private static DataTable MapServicesToTable(List<ProviderServiceBaseAddRequest> servicesToMap)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Price", typeof(decimal));
            dt.Columns.Add("ServiceId", typeof(Int32));
            dt.Columns.Add("ServiceTypeId", typeof(Int32));

            foreach (ProviderServiceBaseAddRequest item in servicesToMap)
            {
                DataRow dr = dt.NewRow();
                int startingIndex = 0;  // column index

                dr.SetField(startingIndex++, item.Price);
                dr.SetField(startingIndex++, item.ServiceId);
                dr.SetField(startingIndex++, item.ServiceTypeId);

                dt.Rows.Add(dr);
            }
            return dt;
        }

        private static DataTable MapSpecializationsToTable(List<ProviderSpecializationAddRequest> specializationsToMap)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SpecializationId", typeof(Int32));
            dt.Columns.Add("IsPrimary", typeof(bool));

            foreach (ProviderSpecializationAddRequest item in specializationsToMap)
            {
                DataRow dr = dt.NewRow();
                int startingIndex = 0;  // column index

                dr.SetField(startingIndex++, item.SpecializationId);
                dr.SetField(startingIndex++, item.IsPrimary);

                dt.Rows.Add(dr);
            }
            return dt;
        }
        #endregion
    }

}

