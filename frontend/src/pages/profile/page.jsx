import React, { useEffect, useState } from 'react';

export default function ProfilePage() {
  const [user, setUser] = useState(null);
  const [error, setError] = useState('');

useEffect(() => {
    const fetchProfile = async () => {
      const token = localStorage.getItem('token');              
      const res = await fetch('http://localhost:5054/api/User/profile', {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,                    
          'Content-Type': 'application/json'
        }
      });
  
      console.log(token)
      if (!res.ok) throw new Error('Failed to fetch profile');
      const data = await res.json();
      setUser(data);
    };
  
    fetchProfile().catch(err => setError(err.message));
  }, []);
  

  if (error) return <p style={{ color: 'red' }}>{error}</p>;
  if (!user) return <p>Loading...</p>;

  return (
    <div>
      <h2>Welcome, {user.name || user.email}</h2>
      <p>Email: {user.email}</p>
      <p>User ID: {user.id}</p>
    </div>
  );
}
