import { GoogleMap, LoadScript, Marker } from "@react-google-maps/api";
import { Autocomplete } from "@react-google-maps/api";
import PropTypes from "prop-types";
import React, { Component } from "react";
import logger from "sabio-debug";
import { REACT_APP_APIKEY } from "../../services/serviceHelpers"; //"@service./serviceHelpers";

const _logger = logger.extend("AutoComplete Search");

var mapOptions = { disableDefaultUI: true };
const mapContainerStyle = {
  height: "400px",
  width: "100%",
  padding: `0 40px`,
};

class AutoCompleteSearch extends Component {
  constructor(props) {
    super(props);
    this.state = {
      position: null,
      center: { lat: 38.685, lng: -115.234 },
      zoom: 2.5,
    };

    this.autocomplete = null;
  }

  onLoad = (autocomplete) => {
    this.autocomplete = autocomplete;
  };
  onMarkerLoad = (marker) => {
    this.marker = marker;
  };
  onPlaceChanged = () => {
    if (this.autocomplete !== null) {
      _logger(this.autocomplete.getPlace());

      const addressComponents = this.autocomplete.getPlace();
      let location = { place: " " };
      const lat = addressComponents.geometry.location.lat();
      const lng = addressComponents.geometry.location.lng();

      addressComponents.address_components.forEach((element) => {
        location[element.types[0]] = element.long_name;
      });

      this.props.onSelectedLocation(location, lat, lng);
      this.setState((prevState) => {
        return {
          ...prevState,
          position: { lat: lat, lng: lng },
          center: { lat: lat, lng: lng },
          zoom: 13,
        };
      });
    }
  };
  render() {
    return (
      <React.Fragment>
        <div>
          <LoadScript
            libraries={["places"]}
            googleMapsApiKey={REACT_APP_APIKEY}
          >
            <GoogleMap
              id="marker-example"
              mapContainerStyle={mapContainerStyle}
              zoom={this.state.zoom}
              center={this.state.center}
              options={mapOptions}
            >
              <Autocomplete
                onLoad={this.onLoad}
                onPlaceChanged={this.onPlaceChanged}
              >
                <div>
                  <input
                    type="text"
                    placeholder="Place Your Address Here"
                    style={{
                      boxSizing: `border-box`,
                      border: `1px solid transparent`,
                      width: `240px`,
                      height: `32px`,
                      padding: `0 12px`,
                      borderRadius: `3px`,
                      boxShadow: `0 2px 6px rgba(0, 0, 0, 0.3)`,
                      fontSize: `14px`,
                      outline: `none`,
                      textOverflow: `ellipses`,
                      position: "absolute",
                      left: "50%",
                      marginLeft: "-120px",
                    }}
                  />
                </div>
              </Autocomplete>
              {this.state.position && (
                <Marker
                  onLoad={this.onMarkerLoad}
                  position={this.state.position}
                />
              )}
            </GoogleMap>
          </LoadScript>
        </div>
      </React.Fragment>
    );
  }
}
AutoCompleteSearch.propTypes = {
  onSelectedLocation: PropTypes.func,
};

export default AutoCompleteSearch;
