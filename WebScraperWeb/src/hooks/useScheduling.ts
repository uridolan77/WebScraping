import { useState, useCallback } from 'react';
import { 
  getAllScheduledTasks,
  getScheduledTask,
  createScheduledTask,
  updateScheduledTask,
  deleteScheduledTask,
  runScheduledTaskNow,
  pauseScheduledTask,
  resumeScheduledTask,
  getTaskExecutionHistory
} from '../api/scheduling';
import { getUserFriendlyErrorMessage } from '../utils/errorHandler';

/**
 * Custom hook for scheduling data and operations
 * @returns {Object} Scheduling data and functions
 */
export const useScheduling = () => {
  const [schedules, setSchedules] = useState([]);
  const [selectedSchedule, setSelectedSchedule] = useState(null);
  const [executionHistory, setExecutionHistory] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  /**
   * Fetch all scheduled tasks
   */
  const fetchSchedules = useCallback(async () => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await getAllScheduledTasks();
      setSchedules(data);
      return data;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to fetch scheduled tasks');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Fetch a scheduled task by ID
   * @param {string} id - Schedule ID
   */
  const fetchSchedule = useCallback(async (id) => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await getScheduledTask(id);
      setSelectedSchedule(data);
      return data;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to fetch scheduled task');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Create a new scheduled task
   * @param {Object} taskData - Schedule data
   */
  const createSchedule = useCallback(async (taskData) => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await createScheduledTask(taskData);
      setSchedules(prev => [...prev, data]);
      return data;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to create scheduled task');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Update an existing scheduled task
   * @param {string} id - Schedule ID
   * @param {Object} taskData - Updated schedule data
   */
  const updateSchedule = useCallback(async (id, taskData) => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await updateScheduledTask(id, taskData);
      
      // Update schedules list
      setSchedules(prev => 
        prev.map(schedule => schedule.id === id ? data : schedule)
      );
      
      // Update selected schedule if it's the one being edited
      if (selectedSchedule && selectedSchedule.id === id) {
        setSelectedSchedule(data);
      }
      
      return data;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to update scheduled task');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, [selectedSchedule]);

  /**
   * Delete a scheduled task
   * @param {string} id - Schedule ID
   */
  const deleteSchedule = useCallback(async (id) => {
    setLoading(true);
    setError(null);
    
    try {
      await deleteScheduledTask(id);
      
      // Remove from schedules list
      setSchedules(prev => prev.filter(schedule => schedule.id !== id));
      
      // Clear selected schedule if it's the one being deleted
      if (selectedSchedule && selectedSchedule.id === id) {
        setSelectedSchedule(null);
      }
      
      return true;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to delete scheduled task');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, [selectedSchedule]);

  /**
   * Run a scheduled task immediately
   * @param {string} id - Schedule ID
   */
  const runNow = useCallback(async (id) => {
    setLoading(true);
    setError(null);
    
    try {
      const result = await runScheduledTaskNow(id);
      
      // Refresh the schedule to get updated lastRun time
      const updatedSchedule = await getScheduledTask(id);
      
      // Update schedules list
      setSchedules(prev => 
        prev.map(schedule => schedule.id === id ? updatedSchedule : schedule)
      );
      
      // Update selected schedule if it's the one being run
      if (selectedSchedule && selectedSchedule.id === id) {
        setSelectedSchedule(updatedSchedule);
      }
      
      return result;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to run scheduled task');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, [selectedSchedule]);

  /**
   * Pause a scheduled task
   * @param {string} id - Schedule ID
   */
  const pauseSchedule = useCallback(async (id) => {
    setLoading(true);
    setError(null);
    
    try {
      const result = await pauseScheduledTask(id);
      
      // Update schedules list
      setSchedules(prev => 
        prev.map(schedule => 
          schedule.id === id ? { ...schedule, status: 'paused' } : schedule
        )
      );
      
      // Update selected schedule if it's the one being paused
      if (selectedSchedule && selectedSchedule.id === id) {
        setSelectedSchedule(prev => ({ ...prev, status: 'paused' }));
      }
      
      return result;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to pause scheduled task');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, [selectedSchedule]);

  /**
   * Resume a scheduled task
   * @param {string} id - Schedule ID
   */
  const resumeSchedule = useCallback(async (id) => {
    setLoading(true);
    setError(null);
    
    try {
      const result = await resumeScheduledTask(id);
      
      // Update schedules list
      setSchedules(prev => 
        prev.map(schedule => 
          schedule.id === id ? { ...schedule, status: 'active' } : schedule
        )
      );
      
      // Update selected schedule if it's the one being resumed
      if (selectedSchedule && selectedSchedule.id === id) {
        setSelectedSchedule(prev => ({ ...prev, status: 'active' }));
      }
      
      return result;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to resume scheduled task');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, [selectedSchedule]);

  /**
   * Toggle schedule status (pause/resume)
   * @param {string} id - Schedule ID
   */
  const toggleScheduleStatus = useCallback(async (id) => {
    // Find the schedule to determine current status
    const schedule = schedules.find(s => s.id === id);
    if (!schedule) {
      throw new Error(`Schedule with ID ${id} not found`);
    }
    
    if (schedule.status === 'active') {
      return pauseSchedule(id);
    } else {
      return resumeSchedule(id);
    }
  }, [schedules, pauseSchedule, resumeSchedule]);

  /**
   * Fetch execution history for a scheduled task
   * @param {string} id - Schedule ID
   * @param {number} limit - Maximum number of history entries to return
   */
  const fetchExecutionHistory = useCallback(async (id, limit = 10) => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await getTaskExecutionHistory(id, limit);
      setExecutionHistory(data);
      return data;
    } catch (err) {
      const errorMessage = getUserFriendlyErrorMessage(err, 'Failed to fetch execution history');
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    schedules,
    selectedSchedule,
    executionHistory,
    loading,
    error,
    fetchSchedules,
    fetchSchedule,
    createSchedule,
    updateSchedule,
    deleteSchedule,
    runNow,
    pauseSchedule,
    resumeSchedule,
    toggleScheduleStatus,
    fetchExecutionHistory
  };
};

export default useScheduling;
