import React from 'react';
import Item from './item.jsx';
import { connect } from 'react-redux';
import axios from 'axios';
import store from './../store.js'
import {listChefToDos} from './../chef.js'

class Chef extends React.Component {
  render () {
    var foodItemsArray = this.props.chefToDos.map(chefToDo => chefToDo.foodItems)

    var foodItems =
      foodItemsArray.length
      ? foodItemsArray
        .reduce((a,b) => a.concat(b))
        .map(foodItem => <Item item={foodItem} key={foodItem.menuNumber} />)
      : []

    return (
        <div>
          {foodItems}
        </div>
    )
  }
}

class ChefContainer extends React.Component {
  componentDidMount(){
    axios.get('/todos/chef').then(response => {
      store.dispatch(listChefToDos(response.data));
    });
  }

  render () {
    return <Chef chefToDos={this.props.chefToDos} />;
  }
}

const mapStateToProps = function(store) {
  return {
    chefToDos: store.chefToDosState.chefToDos
  };
}

export default connect(mapStateToProps)(ChefContainer)