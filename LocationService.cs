using Sabio.Data.Providers;
using Sabio.Models.Requests.Locations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Sabio.Models.Domain;
using Sabio.Data;
using Sabio.Models;


namespace Sabio.Services
{
    public class LocationService : ILocationService   //,ILocationMapper
    {

        IDataProvider _data = null;
        public LocationService(IDataProvider data)
        {
            _data = data;

        }
        public int Add(LocationAddRequest request, int currentUserId)
        {
            int id = 0;

            string procName = "[dbo].[Locations_Insert_V2]";


            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection col)
            {
                InputCommonParams(request, col);
                col.AddWithValue("@CreatedBy", currentUserId);

                SqlParameter idOutPut = new SqlParameter("@Id", SqlDbType.Int);
                idOutPut.Direction = ParameterDirection.Output;
                col.Add(idOutPut);

            }, returnParameters: delegate (SqlParameterCollection returnCol)
            {
                object objectId = returnCol["@Id"].Value;
                int.TryParse(objectId.ToString(), out id);
            });


            return id;
        }
        public void Update(LocationUpdateRequest request, int currentUserId)
        {

            string procName = "[dbo].[Locations_Update_V2]";
            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection col)
              {

                  InputCommonParams(request, col);
                  col.AddWithValue("@ModifiedBy", currentUserId);


                  col.AddWithValue("@Id", request.Id);

              }, returnParameters: null);



        }
        public Location Get(int Id)
        {
            Location locations = null;
            int startingIndex = 0;

            string procName = "[dbo].[Locations_SelectById_V2]";
            _data.ExecuteCmd(procName, delegate (SqlParameterCollection col)
             {
                 col.AddWithValue("@Id", Id);

             }, delegate (IDataReader reader, short set)
             {
                 locations = MapALocation(reader, out startingIndex);

             });


            return locations;

        }
        public Paged<Location> GetAllPaged(int pageIndex, int pageSize)
        {
            Paged<Location> pgResult = null;

            List<Location> result = null;

            int startingIndex = 0;
            int totalCount = 0;
            string procName = "[dbo].[Location_SelectAllByPage_V2]";
            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection paramCol)
                 {
                     paramCol.AddWithValue("@PageIndex", pageIndex);
                     paramCol.AddWithValue("@pageSize", pageSize);

                 },
                singleRecordMapper: delegate (IDataReader reader, short set)
                 {
                     
                     Location locations = MapALocation(reader, out startingIndex);


                     if (totalCount == 0)
                     {
                         totalCount = reader.GetSafeInt32(startingIndex++);
                     };

                     if (result == null)
                     {
                         result = new List<Location>();
                     }
                     result.Add(locations);
                 });
            if (result != null)
            {
                pgResult = new Paged<Location>(result, pageIndex, pageSize, totalCount);
            }
            return pgResult;
        }
        public Paged<Location> GetLocation( int pageIndex, int pageSize, int radius)
        {
            Paged<Location> pgResult = null;

            List<Location> result = null;
            int startingIndex = 0;
            int totalCount = 0;
            string procName = "[dbo].[Locations_Select_ByGeo_V2]";
            _data.ExecuteCmd(procName, delegate (SqlParameterCollection col)
             {
                 
                 col.AddWithValue("@pageIndex", pageIndex);
                 col.AddWithValue("@pageSize", pageSize);
                 col.AddWithValue("@radius", radius);



             }, delegate (IDataReader reader, short set)
             {
                 
                 Location locations = MapALocation(reader, out startingIndex);
                 if (totalCount == 0)
                 {
                     totalCount = reader.GetSafeInt32(startingIndex++);
                 };

                 if (result == null)
                 {
                     result = new List<Location>();
                 }
                 result.Add(locations);
             });
            if (result != null)
            {
                pgResult = new Paged<Location>(result, pageIndex, pageSize, totalCount);
            }
            return pgResult;
        }
        public Paged<Location> GetCurrent(int pageIndex, int pageSize, int currentUserId)
        {
            Paged<Location> pgResult = null;

            List<Location> result = null;

            int startingIndex = 0;
            int totalCount = 0;
            string procName = "[dbo].[Locations_SelectBy_CreatedBy_V2]";
            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection paramCol)
                {
                    paramCol.AddWithValue("@PageIndex", pageIndex);
                    paramCol.AddWithValue("@pageSize", pageSize);
                    paramCol.AddWithValue("@CreatedBy", currentUserId);

                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    Location locations = new Location();

                    

                    locations = MapALocation(reader, out startingIndex);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(startingIndex++);
                    };

                    if (result == null)
                    {
                        result = new List<Location>();
                    }
                    result.Add(locations);
                });
            if (result != null)
            {
                pgResult = new Paged<Location>(result, pageIndex, pageSize, totalCount);
            }
            return pgResult;
        }

        public void Delete(int id)
        {
            string procName = "[dbo].[Locations_DeleteById]";
            _data.ExecuteNonQuery(procName, delegate (SqlParameterCollection col)
             {
                 col.AddWithValue("@Id", id);
             });
        }
        private static void InputCommonParams(LocationAddRequest request, SqlParameterCollection col)
        {
            col.AddWithValue("@LocationTypeId", request.LocationTypeId);
            col.AddWithValue("@LineOne", request.LineOne);
            if (request.LineTwo != null)
            {
                col.AddWithValue("@LineTwo", request.LineTwo);
            }
            else
            {
                col.AddWithValue("@LineTwo", DBNull.Value);
            }
            col.AddWithValue("@City", request.City);
            col.AddWithValue("@Zip", request.Zip);
            col.AddWithValue("@StateId", request.StateId);
            col.AddWithValue("@Latitude", request.Latitude);
            col.AddWithValue("@Longitude", request.Longitude);

        }
        public Location MapALocation(IDataReader reader,out int startingIndex)
        {
            Location locations = new Location();
            locations.LocationType = new LocationType();
            locations.State = new State();

            startingIndex = 0;

            locations.Id = reader.GetSafeInt32(startingIndex++);
            locations.LocationType.Id = reader.GetSafeInt32(startingIndex++);
            locations.LocationType.Name = reader.GetSafeString(startingIndex++);
            locations.LineOne = reader.GetSafeString(startingIndex++);
            locations.LineTwo = reader.GetSafeString(startingIndex++);
            locations.City = reader.GetSafeString(startingIndex++);
            locations.Zip = reader.GetSafeString(startingIndex++);
            locations.State.Id = reader.GetSafeInt32(startingIndex++);
            locations.State.Code = reader.GetSafeString(startingIndex++);
            locations.State.Name = reader.GetSafeString(startingIndex++);
            locations.Latitude = reader.GetSafeDouble(startingIndex++);
            locations.Longitude = reader.GetSafeDouble(startingIndex++);
            

            return locations;
        }



    }

}
