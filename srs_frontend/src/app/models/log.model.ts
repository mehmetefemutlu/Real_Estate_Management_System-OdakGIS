export enum OperationType {
  Create = 1,
  Update = 2,
  Delete = 3,
  Read = 4
}

export enum LogStatus {
  Active = 1,
  Pending = 2,
  Completed = 3,
  Aborted = 4
}

export interface Log {
  id: number;
  userId: number;
  userName: string;
  operationType: OperationType;
  message: string;
  description: string;
  status: LogStatus;
  userIP: string;
  createdAt: string;
}

export interface LogView {
  id: number;
  name: string;
  userId: number;
  operationType: OperationType;
  message: string;
  description: string;
  status: LogStatus;
  userIP: string;
  timestamp: string;
}

export interface CreateLog {
  userId: number;
  operationType: OperationType;
  message: string;
  description: string;
  status: LogStatus;
}

export interface FilterLog {
  userId?: number;
  operationType?: OperationType;
  status?: LogStatus;
  description?: string;
  userIP?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}
