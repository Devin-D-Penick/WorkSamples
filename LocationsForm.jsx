import PropTypes from "prop-types";
import { Formik, Form, ErrorMessage } from "formik";
import AutoCompleteSearch from "./AutoCompleteSearch";
import { WrapperSimple } from "@layout-components";
import React, { Component, Fragment } from "react";
import logger from "sabio-debug";
import Grid from "@material-ui/core/Grid";
import Paper from "@material-ui/core/Paper";
import TextField from "@material-ui/core/TextField";
import Button from "@material-ui/core/Button";
import locationService from "@services/locationService";
import { locationSchema } from "../../schema/locationSchema";
import LocationsDropDown from "@components/locations/LocationsDropDown";
import { Snackbar } from "@material-ui/core";
import Alert from "../../assets/components/Alert";

import toastr from "toastr";

const _logger = logger.extend("Locations Forms");

class LocationsForm extends Component {
  constructor(props) {
    super(props);
    this.state = {
      snackBarShow: false,
      severity: "success",
      barMessage: "",
      location: {
        city: "",
        zip: "",
        lineOne: "",
        lineTwo: "",
        locationTypeId: "",
        state: "",
        stateMenu: [{ states: "", id: 0 }],
        locationMenu: [{ locations: "", id: 0 }],
        stateId: "",
        latitude: "",
        longitude: "",
      },
    };
  }

  onSelectedLocation = (address, lat, lng) => {
    let streetAdd = `${address.street_number} ${address.route}`;
    let latitude = lat;
    let longitude = lng;
    let stateList = this.props.references.states;
    let locationList = this.props.references.locationTypes;

    _logger(stateList, "stateList");
    _logger(locationList, "locationList");

    this.setState((prevState) => {
      let returnedState = stateList.findIndex(
        (stateList) => stateList.name === address.administrative_area_level_1
      );
      let selectedState = stateList[returnedState];
      _logger(returnedState, "Returned State");
      _logger(selectedState, "selectedState");
      return {
        ...prevState,
        location: {
          city: address.locality,
          zip: address.postal_code,
          lineOne: streetAdd,
          lineTwo: null,
          state: address.administrative_area_level_1,
          stateId: selectedState.id,
          stateMenu: stateList,
          locationTypeId: "",
          locationMenu: locationList,
          latitude: latitude,
          longitude: longitude,
        },
      };
    });
  };
  onLocationClick = (e) => {
    let locationList = this.props.references.locationTypes;
    let returnedLocation = locationList.findIndex(
      (locationList) => locationList.name === e.innerText
    );
    let selectedLocation = locationList[returnedLocation];

    this.setState((prevState) => {
      return { ...prevState, locationTypeId: selectedLocation.id };
    });
  };

  handleSubmit = (formValues) => {
    let neededValues = {
      locationTypeId: this.state.locationTypeId,
      lineOne: formValues.lineOne,
      lineTwo: formValues.lineTwo || null,
      city: formValues.city,
      zip: formValues.zip,
      stateId: formValues.stateId,
      latitude: formValues.latitude,
      longitude: formValues.longitude,
    };
    _logger(neededValues, "neededValues");
    _logger(formValues, "formValues");
    locationService
      .createLocation(neededValues)
      .then(this.onSubmitSuccess)
      .catch(this.onSubmitFail);
  };
  onSubmitFail = () => {
    toastr.error("Submit failed, please refresh and try again.");
    _logger("Submit Fail");
  };
  onSubmitSuccess = () => {
    this.setState((prevState) => {
      return {
        ...prevState,
        snackbarShow: true,
        barMessage: "Location Added",
        severity: "success",
      };
    });
    _logger("Submit Hit ");
  };
  handleClose = (event, reason) => {
    if (reason === "clickaway") {
      return;
    }
    this.setState((prevState) => {
      return {
        ...prevState,
        snackbarShow: false,
      };
    });
  };

  render() {
    return (
      <Fragment>
        <Formik
          enableReinitialize={true}
          initialValues={this.state.location}
          onSubmit={this.handleSubmit}
          validationSchema={locationSchema}
        >
          {(formikProps) => {
            const {
              values,
              handleSubmit,
              isSubmitting,
              isValid,
              handleChange,
            } = formikProps;
            return (
              <Grid container spacing={6}>
                <Grid item xs={6}>
                  <WrapperSimple>
                    <Paper>
                      <AutoCompleteSearch
                        onSelectedLocation={this.onSelectedLocation}
                      />
                    </Paper>
                  </WrapperSimple>
                </Grid>
                <Grid item xs={6}>
                  <WrapperSimple>
                    <Form onSubmit={handleSubmit}>
                      <Grid>
                        <TextField
                          fullWidth
                          margin="normal"
                          placeholder="Street Number"
                          name="lineOne"
                          value={values.lineOne}
                        />
                        <ErrorMessage name="streetNumber" />
                      </Grid>
                      <Grid>
                        <TextField
                          fullWidth
                          margin="normal"
                          onChange={handleChange}
                          type="input"
                          autoComplete="off"
                          placeholder="Building, Suite, Apartment, etc(optional)"
                          name="lineTwo"
                          value={values.lineTwo === null ? "" : values.lineTwo}
                        />
                        <ErrorMessage name="lineTwo" />
                      </Grid>
                      <Grid>
                        <TextField
                          fullWidth
                          margin="normal"
                          placeholder="Zipcode"
                          name="zip"
                          value={values.zip}
                        />
                        <ErrorMessage name="zip" />
                      </Grid>
                      <Grid>
                        <TextField
                          fullWidth
                          margin="normal"
                          placeholder="City"
                          name="city"
                          value={values.city}
                        />
                        <ErrorMessage name="city" />
                      </Grid>
                      <Grid>
                        <TextField
                          fullWidth
                          margin="normal"
                          placeholder="State"
                          name="state"
                          value={values.state}
                        />
                        <ErrorMessage name="state" />
                      </Grid>
                      <Grid>
                        <LocationsDropDown
                          locations={values.locationMenu}
                          onChange={this.onLocationClick}
                        />
                      </Grid>
                      <Button
                        type="submit"
                        variant="contained"
                        color="primary"
                        disabled={!isValid || isSubmitting}
                      >
                        Submit
                      </Button>
                    </Form>
                  </WrapperSimple>
                </Grid>
              </Grid>
            );
          }}
        </Formik>
        <Snackbar
          open={this.state.snackbarShow}
          autoHideDuration={3000}
          onClose={this.handleClose}
          anchorOrigin={{ vertical: "top", horizontal: "center" }}
        >
          <Alert severity={this.state.severity}>{this.state.barMessage}</Alert>
        </Snackbar>
      </Fragment>
    );
  }
}

LocationsForm.propTypes = {
  onSelectedLocation: PropTypes.func,

  references: PropTypes.shape({
    locationTypes: PropTypes.arrayOf(
      PropTypes.shape({
        id: PropTypes.number.isRequired,
        name: PropTypes.string.isRequired,
      })
    ),
    states: PropTypes.arrayOf(
      PropTypes.shape({
        code: PropTypes.string,
        id: PropTypes.number.isRequired,
        name: PropTypes.string,
      })
    ),
  }),
};

export default LocationsForm;
