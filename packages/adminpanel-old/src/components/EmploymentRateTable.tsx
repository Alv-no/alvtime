import MaterialTable, { Column, Options } from "material-table";
import React, { useContext } from "react";
import useSWR from "swr";
import { AlvtimeContext } from "../App";
import { norsk } from "./norsk";
import tableIcons from "./tableIcons";
import { globalTableOptions, setCache } from "./Tables";

export default function EmploymentRateTable(props: { user: object }) {
  const user: { id: number } = (props.user as unknown) as {
    id: number;
  };
  const { alvtimeFetcher } = useContext(AlvtimeContext);

  interface RowData{
    title: string;
    field: string;
    editable: string;
    type: string;
    hidden: boolean;
    fromDateInclusive: string;
    toDateInclusive: string;
    ratePercentage: number;
    id: number;
  }

  const columns: Column<RowData>[] = [
    { title: "Stillingsprosent (%)", field: "ratePercentage", editable: "always", type: "numeric", hidden: false },
    { title: "Fra dato (inklusiv)", field: "fromDateInclusive", editable: "always", type: "date", hidden: false },
    { title: "Til dato (inklusiv)", field: "toDateInclusive", editable: "always", type: "date", hidden: false },
    { title: "RateId", field: "id", editable: "never", type: "boolean", hidden: true}
  ];

  const path = `/api/admin/users/${user.id}/employmentrates`;

  const { data, error } = useSWR(path);

  const handleRowAdd = async (newData: RowData) => {
    setCache(path, [...data, { ...newData, user: { id: user.id } }]);
    const addedData = await alvtimeFetcher(path, {
      method: "post",
      body: 
      [
        {
          ratePercentage: newData.ratePercentage,
          fromDateInclusive: newData.fromDateInclusive,
          toDateInclusive: newData.toDateInclusive
        },
      ]
    });
    setCache(path, [...addedData, ...data])
  };

  const handleRowUpdate = async (newData: RowData, oldData: RowData | undefined)  => {
    const dataUpdate = [...data];
    const index = dataUpdate.findIndex((x) => x.id === oldData?.id);
    dataUpdate[index] = newData;
    setCache(path, [...dataUpdate]);
    const updatedData = await alvtimeFetcher(path, {
      method: "put",
      body:
      [ 
        { 
            id: oldData?.id, 
            ratePercentage: newData.ratePercentage,
            fromDateInclusive: newData.fromDateInclusive,
            toDateInclusive: newData.toDateInclusive 
        }
      ],
    });
    dataUpdate[index] = updatedData[0];
    setCache(path, [...dataUpdate]);
  };

  if (error) return <div>Error...</div>;
  const isLoading = !data;
  const filteredData = !data
    ? data
    : data.filter(
        (employmentRate: { userId: number }) =>
          employmentRate.userId === ((user.id as unknown) as number)
      );

  return (
    <MaterialTable
      icons={tableIcons}
      title="Stillingsprosent"
      columns={columns}
      data={filteredData}
      isLoading={isLoading}
      options={globalTableOptions as Options<RowData>}
      editable={{
        onRowAdd: handleRowAdd,
        onRowUpdate: handleRowUpdate,
      }}
      localization={norsk}
    />
  );
}
