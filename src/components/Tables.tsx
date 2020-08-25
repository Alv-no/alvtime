import AddBox from "@material-ui/icons/AddBox";
import ArrowDownward from "@material-ui/icons/ArrowDownward";
import Check from "@material-ui/icons/Check";
import ChevronLeft from "@material-ui/icons/ChevronLeft";
import ChevronRight from "@material-ui/icons/ChevronRight";
import Clear from "@material-ui/icons/Clear";
import DeleteOutline from "@material-ui/icons/DeleteOutline";
import Edit from "@material-ui/icons/Edit";
import FilterList from "@material-ui/icons/FilterList";
import FirstPage from "@material-ui/icons/FirstPage";
import LastPage from "@material-ui/icons/LastPage";
import Remove from "@material-ui/icons/Remove";
import SaveAlt from "@material-ui/icons/SaveAlt";
import Search from "@material-ui/icons/Search";
import ViewColumn from "@material-ui/icons/ViewColumn";
import MaterialTable from "material-table";
import React, { forwardRef, useEffect, useState } from "react";
import { adAuthenticatedFetch } from "../services/azureAd";
import config from "../config";

const tableIcons = {
  //@ts-ignore
  Add: forwardRef((props, ref) => <AddBox {...props} ref={ref} />),
  //@ts-ignore
  Check: forwardRef((props, ref) => <Check {...props} ref={ref} />),
  //@ts-ignore
  Clear: forwardRef((props, ref) => <Clear {...props} ref={ref} />),
  //@ts-ignore
  Delete: forwardRef((props, ref) => <DeleteOutline {...props} ref={ref} />),
  //@ts-ignore
  DetailPanel: forwardRef((props, ref) => (
    //@ts-ignore
    <ChevronRight {...props} ref={ref} />
  )),
  //@ts-ignore
  Edit: forwardRef((props, ref) => <Edit {...props} ref={ref} />),
  //@ts-ignore
  Export: forwardRef((props, ref) => <SaveAlt {...props} ref={ref} />),
  //@ts-ignore
  Filter: forwardRef((props, ref) => <FilterList {...props} ref={ref} />),
  //@ts-ignore
  FirstPage: forwardRef((props, ref) => <FirstPage {...props} ref={ref} />),
  //@ts-ignore
  LastPage: forwardRef((props, ref) => <LastPage {...props} ref={ref} />),
  //@ts-ignore
  NextPage: forwardRef((props, ref) => <ChevronRight {...props} ref={ref} />),
  //@ts-ignore
  PreviousPage: forwardRef((props, ref) => (
    //@ts-ignore
    <ChevronLeft {...props} ref={ref} />
  )),
  //@ts-ignore
  ResetSearch: forwardRef((props, ref) => <Clear {...props} ref={ref} />),
  //@ts-ignore
  Search: forwardRef((props, ref) => <Search {...props} ref={ref} />),
  //@ts-ignore
  SortArrow: forwardRef((props, ref) => <ArrowDownward {...props} ref={ref} />),
  //@ts-ignore
  ThirdStateCheck: forwardRef((props, ref) => <Remove {...props} ref={ref} />),
  //@ts-ignore
  ViewColumn: forwardRef((props, ref) => <ViewColumn {...props} ref={ref} />),
};

export default function CustomerTable() {
  const columns = [
    { title: "Name", field: "name" },
    { title: "contactEmail", field: "contactEmail" },
    { title: "contactPerson", field: "contactPerson" },
    { title: "contactPhone", field: "contactPhone" },
    { title: "id", field: "id" },
    { title: "invoiceAddress", field: "invoiceAddress" },
  ];

  useEffect(() => {
    async function getCustomers() {
      try {
        const url = new URL(
          config.API_HOST + "/api/admin/Customers"
        ).toString();
        const res = await adAuthenticatedFetch(url);
        if (res.status === 404) {
          throw res.statusText;
        }
        const customers = await res.json();
        //setColumns(customers);
        setData(customers);
      } catch (e) {
        if (e !== "Not Found") {
          console.error(e);
        }
      }
    }
    getCustomers();
  }, []);

  const [data, setData] = useState([]);

  return (
    <MaterialTable
      //@ts-ignore
      icons={tableIcons}
      title="Editable Preview"
      columns={columns}
      data={data}
      editable={{
        onRowAdd: (newData) =>
          new Promise((resolve, reject) => {
            setTimeout(() => {
              setData([...data, newData]);

              resolve();
            }, 1000);
          }),
        onRowUpdate: (newData, oldData) =>
          new Promise((resolve, reject) => {
            setTimeout(() => {
              const dataUpdate = [...data];
              //@ts-ignore
              const index = oldData.tableData.id;
              dataUpdate[index] = newData;
              setData([...dataUpdate]);

              resolve();
            }, 1000);
          }),
        onRowDelete: (oldData) =>
          new Promise((resolve, reject) => {
            setTimeout(() => {
              const dataDelete = [...data];
              //@ts-ignore
              const index = oldData.tableData.id;
              dataDelete.splice(index, 1);
              setData([...dataDelete]);

              resolve();
            }, 1000);
          }),
      }}
    />
  );
}
