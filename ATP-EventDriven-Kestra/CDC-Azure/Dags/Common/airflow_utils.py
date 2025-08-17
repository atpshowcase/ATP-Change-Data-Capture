import os

def is_dag_exists(dag_id):
    # Pengecekan apakah DAG sudah ada berdasarkan file yang ada
    dags_folder = "/path/to/your/airflow/dags"
    for filename in os.listdir(dags_folder):
        if filename.endswith(".py") and dag_id in filename:
            return True
    return False
