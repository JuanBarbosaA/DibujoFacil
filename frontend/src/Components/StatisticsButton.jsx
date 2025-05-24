import React from 'react';
import axios from 'axios';

const StatisticsButton = () => {
    const handleDownload = async () => {
        try {
          const token = localStorage.getItem('token');
      
          const response = await axios.get('http://localhost:5054/api/admin/users/statistics/export', {
            responseType: 'blob',
            headers: {
              Authorization: `Bearer ${token}`
            }
          });
      
          const blob = new Blob([response.data], {
            type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
          });
      
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.setAttribute('download', 'EstadisticasUsuarios.xlsx');
          document.body.appendChild(link);
          link.click();
      
          link.parentNode.removeChild(link);
          window.URL.revokeObjectURL(url);
      
        } catch (error) {
          console.error('Error descargando reporte:', error);
          alert('Error al generar el reporte');
        }
      };
      
  return (
    <button 
      onClick={handleDownload}
      style={{
        padding: '10px 20px',
        backgroundColor: '#4CAF50',
        color: 'white',
        border: 'none',
        borderRadius: '4px',
        cursor: 'pointer'
      }}
    >
      Generar Reporte de Estadísticas
    </button>
  );
};

export default StatisticsButton;