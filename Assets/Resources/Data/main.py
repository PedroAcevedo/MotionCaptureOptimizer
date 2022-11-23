import numpy as np
import pandas as pd
from scipy.stats import ttest_ind

props = ['Umbrella','Pingpong','Backpack','Bottle','Glass','Box','Hammer','Sword','Shield','Bike']

def get_average_results(folder, conditions):
    
    df = pd.DataFrame({ "prop": props })

    for test in conditions:
        average_per_condition = pd.DataFrame({ "prop": props })
        for i in range(10):
            data = pd.read_csv(f"{folder}/final_{test}_{i+1}_results.csv")
            average_per_condition["final_" + str(test) + "_" + str(i)] = data["Optimal"]
        average_per_condition['mean_' + str(test)] = average_per_condition.iloc[:, 1:6].mean(axis=1)
        df['mean_' + str(test)] = average_per_condition['mean_' + str(test)]

    df.to_csv(f"{folder}/final_results.csv")

    return df

def get_average_results_for_prop(folder, conditions, prop):
    
    selected_prop = props[prop]
    print(selected_prop)

    prop_data = { "treatment": [], "costs": [] }

    for test in conditions:
        for i in range(10):
            prop_data["treatment"].append(test)
            data = pd.read_csv(f"{folder}/final_{test}_{i+1}_results.csv")
            prop_data["costs"].append(data["Optimal"].iloc[prop])

    df = pd.DataFrame(prop_data)
    df.to_csv(f"{folder}/{selected_prop}_final_results.csv", index=False)



def get_p_value(df):
    _, p = ttest_ind(df[df.columns[1]], df[df.columns[2]])
    print(f"The p-value of the data is {p}")


get_average_results("NumberOfMarkers", ["5_marker","10_marker","15_marker"]) #repetead mesure ANOVA
get_average_results("Positions", ["4x4x4","8x8x8"]) #PER SAMPLE T test
get_average_results("Constrains", ["constrained","unconstrained"]) 

# for i in range(len(props)):
#     get_average_results_for_prop("NumberOfMarkers", ["5_marker","10_marker","15_marker"], i)
#get_p_value(df)
