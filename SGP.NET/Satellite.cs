﻿using System;
using System.Collections.Generic;

namespace SGPdotNET
{
    /// <summary>
    ///     A representation of a satellite in orbit
    /// </summary>
    public class Satellite
    {
        private readonly Sgp4 _sgp4;

        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="tle1">The first line of the set</param>
        /// <param name="tle2">The second line of the set</param>
        public Satellite(string tle1, string tle2) : this("Unnamed", tle1, tle2)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="name">The name of the satellite</param>
        /// <param name="tle1">The first line of the set</param>
        /// <param name="tle2">The second line of the set</param>
        public Satellite(string name, string tle1, string tle2)
        {
            _sgp4 = new Sgp4(new Tle(name, tle1, tle2));

            Name = name;
        }

        /// <summary>
        ///     The name of this satellite
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Predicts the satellite's real-time location
        /// </summary>
        /// <returns>An ECI coordinate set representing the satellite</returns>
        public Eci Predict()
        {
            return Predict(DateTime.UtcNow);
        }

        /// <summary>
        ///     Predicts the satellite's location at a specific time
        /// </summary>
        /// <param name="time">The time of observation</param>
        /// <returns>An ECI coordinate set representing the satellite at the given time</returns>
        public Eci Predict(DateTime time)
        {
            return _sgp4.FindPosition(time);
        }

        /// <summary>
        ///     Gets a list of geodetic coordinates which define the bounds of the visibility footprint
        /// </summary>
        /// <returns>A list of geodetic coordinates</returns>
        public List<ICoordinate> GetFootprint()
        {
            return GetFootprint(DateTime.UtcNow);
        }

        /// <summary>
        ///     Gets a list of geodetic coordinates which define the bounds of the visibility footprint at a specific time
        /// </summary>
        /// <param name="time">The time to predict the footprint</param>
        /// <param name="numPoints">The number of points in the resulting circle</param>
        /// <returns>A list of geodetic coordinates for the specified time</returns>
        public List<ICoordinate> GetFootprint(DateTime time, int numPoints = 60)
        {
            var center = Predict(time).ToGeodetic();
            var coords = new List<ICoordinate>();

            var lat = center.Latitude;
            var lon = center.Longitude;
            var d = center.GetFootprintRadians();

            for (var i = 0; i < numPoints; i++)
            {
                var perc = i / (float)numPoints * 2 * Math.PI;

                var latRadians = Math.Asin(Math.Sin(lat) * Math.Cos(d) + Math.Cos(lat) * Math.Sin(d) * Math.Cos(perc));
                var lngRadians = lon + Math.Atan2(Math.Sin(perc) * Math.Sin(d) * Math.Cos(lat), Math.Cos(d) - Math.Sin(lat) * Math.Sin(latRadians));

                coords.Add(new CoordGeodetic(latRadians, lngRadians, 10, true));
            }

            return coords;
        }
    }
}